package main

import (
  "errors"
  "github.com/emersion/go-sasl"
  "github.com/emersion/go-smtp"
  "io"
  "io/ioutil"
  "log"
  "strings"
)

// The Backend implements SMTP server methods.
type Backend struct{}

func (b *Backend) Login(state *smtp.ConnectionState, username, password string) (smtp.Session, error) {
  if username != "user@example.com" || password != "password" {
    return nil, errors.New("Invalid username or password")
  }

  return &Session{}, nil
}

func (b *Backend) AnonymousLogin(state *smtp.ConnectionState) (smtp.Session, error) {
  //return nil, errors.New("Anonymous login not supported")
  return &Session{}, nil
}

// A Session is returned after EHLO.
type Session struct{}

func (s *Session) Mail(from string, opts smtp.MailOptions) error {
  log.Println("Mail from:", from)
  return nil
}

func (s *Session) Rcpt(to string) error {
  log.Println("Rcpt to:", to)
  return nil
}

func (s *Session) Data(r io.Reader) error {
  if b, err := ioutil.ReadAll(r); err != nil {
    return err
  } else {
    log.Println("Data:", string(b))
  }
  return nil
}

func (s *Session) Reset() {}

func (s *Session) Logout() error {
  return nil
}

func main() {
  //be := &Backend{}
  //
  //s := smtp.NewServer(be)
  //
  //s.Addr = ":1025"
  //s.Domain = "localhost"
  //s.ReadTimeout = 10 * time.Second
  //s.WriteTimeout = 10 * time.Second
  //s.MaxMessageBytes = 1024 * 1024
  //s.MaxRecipients = 50
  //s.AllowInsecureAuth = true
  //s.EnableAuth(sasl.Anonymous, func(conn *smtp.Conn) sasl.Server {
  //	return sasl.NewAnonymousServer(func(trace string) error {
  //		conn.SetSession(&Session{})
  //		return nil
  //	})
  //})
  //
  //s.Debug = log.Writer()
  //
  //args := os.Args[1:]
  //var ports []string
  //if len(args) == 0 {
  //	ports = []string{":1025"}
  //} else {
  //	ports = args
  //}
  //
  //var wg = new(sync.WaitGroup)
  //wg.Add(len(ports))
  //for _, port := range ports {
  //	go func(p string) {
  //		defer wg.Done()
  //		log.Println("Starting server at " + p)
  //		listener, err := net.Listen("tcp", "0.0.0.0"+p)
  //		if err != nil {
  //			log.Fatal(err)
  //		}
  //
  //		if err := s.Serve(listener); err != nil {
  //			log.Fatal(err)
  //		}
  //
  //	}(port)
  //}
  //
  //wg.Wait()
  // Setup authentication information.

  auth := sasl.NewAnonymousClient("0xFF@centurion.gg")
  //auth := sasl.NewPlainClient("", "user@example.com", "password")

  // Connect to the server, authenticate, set the sender and recipient,
  // and send the email all in one step.
  to := []string{"recipient@example.net"}
  msg := strings.NewReader("To: recipient@example.net\r\n" +
    "Subject: discount Gophers!\r\n" +
    "\r\n" +
    "This is the email body.\r\n")
  err := smtp.SendMail("mailhandler.centurion.gg:1025", auth, "sender@example.org", to, msg)
  if err != nil {
    log.Fatal(err)
  }
}
