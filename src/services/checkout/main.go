package main

import (
  "context"
  "flag"
  "fmt"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/monitor"
  "github.com/CenturionLabs/centurion/checkout-service/integration"
  "github.com/CenturionLabs/centurion/checkout-service/logging/es"
  "github.com/CenturionLabs/centurion/checkout-service/services"
  "github.com/CenturionLabs/centurion/checkout-service/sites/amazon"
  "github.com/CenturionLabs/centurion/checkout-service/sites/fake_shop"
  "github.com/CenturionLabs/centurion/checkout-service/sites/yeezysupply"
  "github.com/CenturionLabs/centurion/checkout-service/web"
  "github.com/joho/godotenv"
  "github.com/olivere/elastic/v7"
  "github.com/sirupsen/logrus"
  log "github.com/sirupsen/logrus"
  "go.elastic.co/apm/module/apmgrpc"
  "go.elastic.co/ecslogrus"
  "google.golang.org/grpc"
  "math"
  "math/rand"
  "net"
  "os"
  "strconv"
  "strings"
  "time"
)

func init() {
	rand.Seed(time.Now().UnixNano())
	_ = godotenv.Load(".env")

	logLvl := logrus.InfoLevel
	if len(os.Getenv("DEBUG_LOGGING")) > 0 {
		logrus.Println("Verbose logging mode")
		logLvl = logrus.TraceLevel
	}

	logrus.SetLevel(logLvl)
  if len(os.Getenv("LOGSTORAGE_DISABLE_ELASTIC")) > 0 {
    return
  }

	logrus.SetFormatter(&ecslogrus.Formatter{
		DataKey: "labels",
	})
	esclient, err := elastic.NewClient(
		elastic.SetURL(os.Getenv("ELASTICSEARCH_URL")),
		elastic.SetBasicAuth(os.Getenv("ELASTICSEARCH_USERNAME"), os.Getenv("ELASTICSEARCH_PASSWORD")),
		elastic.SetSniff(false))

	if err != nil {
		panic(err)
	}
  esIxName := strings.ToLower(os.Getenv("ELASTICSEARCH_INDEX"))
  hook, err := es.NewAsyncElasticHook(esclient, os.Getenv("ELOGRUS_HOST"), logLvl, esIxName)
	if err != nil {
		panic(err)
	}
	logrus.AddHook(hook)
}

func main() {
  /*provinceCode := "NY"
  phoneNumber := "9179303948"
  holderName := "Samuel Mari"
  profile := &contracts.ProfileData{
    Id:        "",
    FirstName: "Samuel",
    LastName:  "Mari",
    ShippingAddress: &contracts.AddressData{
      Line1: "671 CHurch avenu",
      //Line2:        "",
      CountryId:    "",
      City:         "Woodmere",
      ProvinceCode: &provinceCode,
      ZipCode:      "11598",
    },
    BillingAddress:    &contracts.AddressData{
      Line1: "671 CHurch avenu",
      //Line2:        "",
      CountryId:    "",
      City:         "Woodmere",
      ProvinceCode: &provinceCode,
      ZipCode:      "11598",
    },
    PhoneNumber:       &phoneNumber,
    Email:             "schalkadela@gmail.com",
    Billing:           &contracts.BillingData{
      CardNumber:      "4767718447710783",
      ExpirationMonth: 3,
      ExpirationYear:  2027,
      Cvv:             "873",
      HolderName:      &holderName,
    },
    Name:              "",
    BillingAsShipping: false,
  }

  encryptionResult, err := yeezysupply.Encrypt(profile)
  println(encryptionResult)*/

  port := flag.Int("port", 5007, "The server port")
  flag.Parse()
	amqpConn := os.Getenv("CONNECTIONSTRINGS__RABBITMQ")
	if len(amqpConn) == 0 {
		log.Fatal("no amqp conn provided")
	}

	monitorUrl := os.Getenv("CONNECTIONSTRINGS__MONITOR_URL")
	if len(monitorUrl) == 0 {
		log.Fatal("no monitor url provided")
	}

	lis, err := net.Listen("tcp", fmt.Sprintf("0.0.0.0:%d", *port))
	if err != nil {
		log.Fatalf("unable to listen %d, err: %v", *port, err)
	}

	var grpcOptions []grpc.ServerOption

	grpcOptions = append(grpcOptions, grpc.UnaryInterceptor(apmgrpc.NewUnaryServerInterceptor(apmgrpc.WithRecovery())))
	grpcOptions = append(grpcOptions, grpc.StreamInterceptor(apmgrpc.NewStreamServerInterceptor(apmgrpc.WithRecovery())))
	GrpcMaxMessageSize := int(math.Pow(1024, 2)) * 300
	grpcOptions = append(grpcOptions, grpc.MaxRecvMsgSize(GrpcMaxMessageSize))
	grpcOptions = append(grpcOptions, grpc.MaxSendMsgSize(GrpcMaxMessageSize))

	server := grpc.NewServer(grpcOptions...)

	taskFactory := services.NewCheckoutTaskFactory(services.NewModuleMetadataFactory())
	lockFactory := services.NewInMemLock()

	var dialOpts []grpc.DialOption
	dialOpts = append(dialOpts, grpc.WithInsecure())
	dialOpts = append(dialOpts, grpc.WithUnaryInterceptor(apmgrpc.NewUnaryClientInterceptor()))
	dialOpts = append(dialOpts, grpc.WithStreamInterceptor(apmgrpc.NewStreamClientInterceptor()))
	monitorClient := newMonitorClient(monitorUrl, dialOpts)

	rpcManagerFactory := services.NewRpcManagerFactory()
	reCaptchaProvider := services.NewReCaptchaSolverProvider(rpcManagerFactory)
	amazon.Register(taskFactory, lockFactory, monitorClient)
	fake_shop.Register(taskFactory, rpcManagerFactory)
	yeezysupply.Register(taskFactory, lockFactory, rpcManagerFactory, reCaptchaProvider)

	eventDispatcher, err := integration.NewRmqEventsDispatcher(amqpConn)
	if err != nil {
		log.Fatal(err)
	}

	checkoutServer := web.NewCheckoutServer(services.NewCheckoutService(), eventDispatcher, taskFactory, rpcManagerFactory)

	checkout.RegisterCheckoutServer(server, checkoutServer)

	log.Println("Checkout service listening at *:" + strconv.Itoa(*port))
	err = server.Serve(lis)
	if err != nil {
		log.Fatal(err)
	}

	// Create a deadline to wait for.
	_, cancel := context.WithTimeout(context.Background(), time.Second*10)
	defer cancel()

	log.Println("Shutting down")
	//apm.DefaultTracer.Flush(make(chan struct{}))
	os.Exit(0)
}

func newMonitorClient(monitorUrl string, dialOpts []grpc.DialOption) monitor.MonitorClient {
	conn, err := grpc.Dial(monitorUrl, dialOpts...)
	if err != nil {
		log.Fatalln(err)
	}

	monitorClient := monitor.NewMonitorClient(conn)
	return monitorClient
}

func newCryptoClient(cryptoUrl string, dialOpts []grpc.DialOption) checkout.CryptoClient {
	conn, err := grpc.Dial(cryptoUrl, dialOpts...)
	if err != nil {
		log.Fatalln(err)
	}

	monitorClient := checkout.NewCryptoClient(conn)
	return monitorClient
}
