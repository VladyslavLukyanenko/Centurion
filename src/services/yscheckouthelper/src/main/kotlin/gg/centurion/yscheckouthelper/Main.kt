package gg.centurion.yscheckouthelper

import gg.centurion.yscheckouthelper.ConscryptSSLEngineOptions.Attribute
import gg.centurion.yscheckouthelper.web.grpc.CryptoService
import io.grpc.Server
import io.grpc.ServerBuilder
import io.vertx.core.AbstractVerticle
import io.vertx.core.DeploymentOptions
import io.vertx.core.Promise
import io.vertx.core.Vertx
import io.vertx.core.http.Http2Settings
import io.vertx.core.http.HttpVersion
import io.vertx.core.json.JsonObject
import io.vertx.core.net.JdkSSLEngineOptions
import io.vertx.core.net.SSLEngineOptions
import io.vertx.ext.web.client.WebClient
import io.vertx.ext.web.client.WebClientOptions
import java.io.IOException
import java.util.concurrent.TimeUnit
import javax.net.ssl.SSLEngine


object Main {
  @Throws(IOException::class, InterruptedException::class)
  @JvmStatic
  fun main(args: Array<String>) {
    /*val profile = Profile().also {
      it.firstName = "Samuel"
      it.lastName = "Mari"
      it.email = "schalkadela@gmail.com"
      it.phone = "9179303948"
      it.cvv = "873"
      it.cardNumber = "4767718447710783"
      it.expiryYear = "2027"
      it.expiryMonth = "03"
      it.address1 = "671 CHurch avenu"
//      it.address2 = ""
      it.city = "Woodmere"
      it.zip = "11598"
      it.state = "NY"
    }

    var profileJson = BillingShipping.createJson(profile)


    var a = ThreadLocalRandom.current().nextLong((-2010761666L xor -1623898255555L), -746824695L xor -1624784935579L)


    val utag = Utag("UA", "SKU")
    val jar = mutableMapOf<String, Any?>()
    var cookieJar16 = jar
    jar.put("geo_country", "US")
    jar.put("utag_main", utag.genUtagMain())
    var threadLocalRandom10002 = ThreadLocalRandom.current()
    cookieJar16.put(
      "_ga",
      "GA1.2." + threadLocalRandom10002.nextInt(1207338862, 1992599043) + "." + System.currentTimeMillis(),
    )
    cookieJar16 = jar
    threadLocalRandom10002 = ThreadLocalRandom.current()
    cookieJar16.put(
      "_gid",
      "GA1.2." + threadLocalRandom10002.nextInt(120016221, 190016221) + "." + System.currentTimeMillis(),
    )
    jar.put("_gat_tealium_0", "1")
    cookieJar16 = jar
    val long17 = Instant.now().toEpochMilli()
    cookieJar16.put(
      "_fbp",
      "fb.1." + long17 + ThreadLocalRandom.current().nextInt(1000) + "." + Instant.now().toEpochMilli(),
    )
    jar.put("_gcl_au", "1.1." + System.currentTimeMillis() + "." + System.currentTimeMillis())
    jar.put("AMCVS_7ADA401053CCF9130A490D4C%40AdobeOrg", "1")
    val long9 = Instant.now().epochSecond + (748893424L xor 748890320L)
    val long11 = long9 + (-1444572789L xor -1444114453L)
    cookieJar16 = jar
    val int18: Int = Utag.getIntH()
    cookieJar16.put(
      "AMCV_7ADA401053CCF9130A490D4C%40AdobeOrg",
      "-227196251%7CMCIDTS%7C" + int18 + "%7CMCMID%7C" + Utag.getStringr() + "%7CMCAAMLH-" + long11 + "%7C7%7CMCAAMB-" + long11 + "%7CRKhpRz8krg2tLO6pguXWp5olkAcUniQYPHaMWWgdJ3xzPWQmdj0y%7CMCOPTOUT-" + long9 + "s%7CNONE%7CMCAID%7CNONE",
    )
    jar.put("s_cc", "true")
    val long13 = ThreadLocalRandom.current().nextLong(-2010761666L xor -1623898255555L, -746824695L xor -1624784935579L)
    jar.put(
      "s_pers",
      "%20s_vnum%3D" + long13 + "%2526vn%253D1%7C" + long13 + "%3B%20s_invisit%3Dtrue%7C" + (Instant.now()
        .toEpochMilli() + (-1392183605L xor -1390444149L)) + "%3B",
    )

    println(JsonObject(jar).toString())
    return*/
/*
    val profile = Profile.ProfileData.newBuilder()
      .setFirstName("Samuel")
      .setLastName("Mari")
      .setEmail("samalama10@gmail.com")
      .setPhoneNumber("9176606115")
      .setBilling(Profile.BillingData.newBuilder()
        .setCvv("641")
        .setCardNumber("4536410189800572")
        .setHolderName("Samuel Mari")
        .setExpirationYear(2027)
        .setExpirationMonth(8))

    val json = Adyen12().serializeProfileToJson(profile.build())

var cse = Adyen12().serializeAndEncryptProfile(profile.build())

*/
    /*val vertx = Vertx.vertx()
    val options = DeploymentOptions()
//    options.instances = 1


    vertx.deployVerticle(WebClientVerticle(), options) { startResult ->
      println("API Ready")
    }*/

    var portStr = System.getenv("SERVER.PORT")
    if (portStr == null) {
      portStr = "5010"
    }
    val port = portStr.toInt()
    val srv: Server = ServerBuilder.forPort(port)
      .addService(CryptoService())
      .build()

    println("Listening on $port")
    srv.start()
      .awaitTermination()
  }
}

class WebClientVerticle : AbstractVerticle() {
  private fun executeRequest(vertx: Vertx) {
    val clientOptions = createChromeOptions("fakeshop.centurion.gg", 443)
    val webClient = WebClient.create(vertx, clientOptions)

    webClient.get("/fakeproduct")
      .send()
      .onSuccess {
        print(it.bodyAsString())
      }
      .onFailure { it.printStackTrace() }
  }

  private fun createWebClientOptions(host: String, port: Int): WebClientOptions? {
    val clientOptions: WebClientOptions = WebClientOptions()
      .setInitialSettings(
        Http2Settings()
          .setHeaderTableSize(65536)
          .setPushEnabled(true)
          .setMaxConcurrentStreams(1000L)
          .setInitialWindowSize(6291456)
          .setMaxFrameSize(16384)
          .setMaxHeaderListSize(262144)
      )
      .setLogActivity(false)
      .setUserAgentEnabled(false)
      .setProtocolVersion(HttpVersion.HTTP_2)
      .setSsl(true)
      .setUseAlpn(true)
      .setTrustAll(false)
      .setVerifyHost(true)
      .setForceSni(true)
      .setDefaultPort(port)
      .setDefaultHost(host)

    val sslEngineOptions = ConscryptSSLEngineOptions()
      .setAttributeFluent(Attribute.BROTLI, true)
      .setAttributeFluent(Attribute.GREASE, true)
      .setAttributeFluent(Attribute.SESSION_TICKET, true)
      .setAttributeFluent(Attribute.SIGNED_CERT_TIMESTAMPS, true)

    val toRemoveAlg = ShortArray(1)
    toRemoveAlg[0] = 513.toShort()

    clientOptions.setSslEngineOptions(sslEngineOptions.setRemovedSigAlgsFluent(toRemoveAlg))
      .addEnabledSecureTransportProtocol("TLSv1.3")
      .addEnabledSecureTransportProtocol("TLSv1.2")
      .addEnabledSecureTransportProtocol("TLSv1.1")
      .addEnabledSecureTransportProtocol("TLSv1.0")
      .addEnabledCipherSuite("TLS_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_CHACHA20_POLY1305_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_128_CBC_SHA")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_256_CBC_SHA")

    return clientOptions
    return WebClientOptions()
      .setInitialSettings(
        Http2Settings()
          .setHeaderTableSize(65536)
          .setPushEnabled(true)
          .setMaxConcurrentStreams(1000)
          .setInitialWindowSize(6291456)
          .setMaxFrameSize(16384)
          .setMaxHeaderListSize(262144)
      )
      .setSsl(true)
      .setLogActivity(false)
      .setUserAgentEnabled(false)
      .setProtocolVersion(HttpVersion.HTTP_2)
      .setUseAlpn(true)
      .setTrustAll(false)
      .setConnectTimeout(150000)
      .setSslHandshakeTimeoutUnit(TimeUnit.SECONDS)
      .setSslHandshakeTimeout(150)
      .setIdleTimeoutUnit(TimeUnit.SECONDS)
      .setIdleTimeout(150)
      .setKeepAlive(true)
      .setKeepAliveTimeout(30)
      .setHttp2KeepAliveTimeout(100)
      .setHttp2MaxPoolSize(150)
      .setHttp2MultiplexingLimit(200)
      .setPoolCleanerPeriod(15000)
      .setMaxPoolSize(150)
      .setTryUseCompression(true)
      .setTcpFastOpen(true)
      .setTcpKeepAlive(true)
      .setTcpNoDelay(true)
      .setTcpQuickAck(true)
      //    .setDefaultPort(18081)
      .setFollowRedirects(false)
  }

  fun createChromeOptions(host: String, port: Int): WebClientOptions {
    val webClientOptions10000: WebClientOptions = WebClientOptions()
      .setInitialSettings(
        Http2Settings()
          .setHeaderTableSize(-1908753238L xor -1908687702L)
          .setPushEnabled(true)
          .setMaxConcurrentStreams(-2075156740L xor -2075157228L)
          .setInitialWindowSize(6291456).setMaxFrameSize(16384)
          .setMaxHeaderListSize(1306464451L xor 1306202307L)
      )
      .setLogActivity(false)
      .setUserAgentEnabled(false)
      .setProtocolVersion(HttpVersion.HTTP_2)
      .setSsl(true)
      .setUseAlpn(true)
      .setTrustAll(false)
      .setVerifyHost(true)
      .setForceSni(true)
      .setDefaultPort(port)
      .setDefaultHost(host)
//      .setSplitCookies(true)
    val conscryptSSLEngineOptions10001 = ConscryptSSLEngineOptions()
      .setAttributeFluent(Attribute.BROTLI, true)
      .setAttributeFluent(Attribute.GREASE, true)
      .setAttributeFluent(Attribute.SESSION_TICKET, true)
      .setAttributeFluent(Attribute.SIGNED_CERT_TIMESTAMPS, true)

    val short10002 = ShortArray(1)
    short10002[0] = 513.toShort()
    return webClientOptions10000.setSslEngineOptions(conscryptSSLEngineOptions10001
      .setRemovedSigAlgsFluent(short10002))
      .addEnabledSecureTransportProtocol("TLSv1.3")
      .addEnabledSecureTransportProtocol("TLSv1.2")
      .addEnabledSecureTransportProtocol("TLSv1.1")
      .addEnabledSecureTransportProtocol("TLSv1.0")
      .addEnabledCipherSuite("TLS_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_CHACHA20_POLY1305_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA")
      .addEnabledCipherSuite("TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_128_GCM_SHA256")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_256_GCM_SHA384")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_128_CBC_SHA")
      .addEnabledCipherSuite("TLS_RSA_WITH_AES_256_CBC_SHA")
  }

  override fun start(startFuture: Promise<Void>?) {
//    val options = HttpServerOptions()
//    options.port = Integer.getInteger("http.port", 18081)
//    vertx.createHttpServer(
//      options
//    )
//      .requestHandler {
//        it.response().write("Works")
//        it.end()
//      }
//      .exceptionHandler { println(it) }
//      .listen { result ->
//        if (result.succeeded()) {
//          startFuture?.complete()
//          println("Vertx Listening on ${options.port}")
//        } else {
//          startFuture?.fail(result.cause())
//        }
//      }

    executeRequest(vertx)
  }

}

class ConscryptSSLEngineOptions : SSLEngineOptions {
  var attributes: HashMap<String?, Boolean?> = HashMap()
  var removedSigAlgs = ShortArray(0)
    private set

  constructor() {}
  constructor(json: JsonObject?) {}
  constructor(that: JdkSSLEngineOptions?) {}

  fun toJson(): JsonObject {
    return JsonObject()
  }

  fun setRemovedSigAlgsFluent(algs: ShortArray): ConscryptSSLEngineOptions {
    removedSigAlgs = algs
    return this
  }

  fun setAttributesFluent(attributes: HashMap<String?, Boolean?>): ConscryptSSLEngineOptions {
    this.attributes = attributes
    return this
  }

  fun setAttribute(attribute: Attribute, value: Boolean) {
    attributes[attribute.name] = value
  }

  fun setAttributeFluent(
    attribute: Attribute,
    value: Boolean
  ): ConscryptSSLEngineOptions {
    setAttribute(attribute, value)
    return this
  }

  override fun copy(): SSLEngineOptions {
    return ConscryptSSLEngineOptions().setAttributesFluent(attributes).setRemovedSigAlgsFluent(removedSigAlgs)
  }

  companion object {
    private var jdkAlpnAvailable: Boolean? = null

    @get:Synchronized
    val isAlpnAvailable: Boolean
      get() {
        if (jdkAlpnAvailable == null) {
          var available = false
          try {
            SSLEngine::class.java.getDeclaredMethod("getApplicationProtocol")
            available = true
          } catch (var8: Exception) {
            try {
              JdkSSLEngineOptions::class.java.classLoader.loadClass("sun.security.ssl.ALPNExtension")
              available = true
            } catch (var7: Exception) {
            }
          } finally {
            jdkAlpnAvailable = available
          }
        }
        return jdkAlpnAvailable!!
      }
  }


  enum class Attribute {
    GREASE, BROTLI, SESSION_TICKET, SIGNED_CERT_TIMESTAMPS, CERT_TRANSPARENCY
  }
}
