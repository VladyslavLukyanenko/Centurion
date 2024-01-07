package gg.centurion.yscheckouthelper.crypto

import gg.centurion.contract.Profile.ProfileData
import io.vertx.core.json.JsonObject
import org.bouncycastle.jce.provider.BouncyCastleProvider
import java.math.BigInteger
import java.security.*
import java.security.spec.RSAPublicKeySpec
import java.text.SimpleDateFormat
import java.util.*
import javax.crypto.*
import javax.crypto.spec.IvParameterSpec

class Adyen12 {
  /* // $FF: renamed from: 4 java.lang.String
   var f_String_4: String? = null

   // $FF: renamed from: 3 java.lang.String
   var f_String_3: String? = null

   // $FF: renamed from: 5 java.lang.String
   var f_String_5: String? = null

   // $FF: renamed from: 2 java.lang.String
   var f_String_2: String? = null

   // $FF: renamed from: 6 java.lang.String
   var f_String_6: String? = null

   // $FF: renamed from: 0 java.lang.String
   var f_String_0: String? = null

   // $FF: renamed from: c java.lang.String
   var f_String_c: String? = null

   // $FF: renamed from: 7 java.lang.String
   var f_String_7: String? = null

   // $FF: renamed from: 1 java.lang.String
   var f_String_1: String? = null*/

  // $FF: renamed from: c java.security.PublicKey
  var publicKey: PublicKey? = null

  // $FF: renamed from: 0 javax.crypto.Cipher
  var AESCmmNoPadding: Cipher? = null

  // $FF: renamed from: c javax.crypto.Cipher
  var RSASignKeyPKCS1Padding: Cipher? = null

  // $FF: renamed from: 0 (int) javax.crypto.SecretKey
  fun generateAESKey(int0: Int): SecretKey {
    val keyGenerator1: KeyGenerator = KeyGenerator.getInstance("AES")
    return if (!assertionsDisabled/* && keyGenerator1 == null*/) {
      throw AssertionError()
    } else {
      keyGenerator1.init(int0)
      keyGenerator1.generateKey()
    }
  }

  // $FF: renamed from: 0 (io.trickle.profile.Profile) java.lang.String
  fun serializeAndEncryptProfile(profile: ProfileData): String {
    val string1 = serializeProfileToJson(profile)
    initializeSignCryptoKeys()
    return encrypt(string1)
  }

  // $FF: renamed from: c (io.trickle.profile.Profile) java.lang.String
  private fun serializeProfileToJson(profile0: ProfileData): String {
    val json = JsonObject()
    json.put("cvc", profile0.billing.cvv)
    json.put("dfValue", null)
    json.put("expiryMonth", profile0.billing.expirationMonth.toString())
    json.put("expiryYear", profile0.billing.expirationYear.toString())
    json.put("generationtime", dateFormat.format(Date()))
    json.put("holderName", profile0.billing.holderName)
    json.put("initializeCount", "1")
    json.put("luhnCount", "1")
    json.put("luhnOkCount", "1")
    json.put("luhnSameLengthCount", "1")
    json.put("number", profile0.billing.cardNumber)
    json.put("sjclStrength", "10")

    return json.toString()
  }

  // $FF: renamed from: c () void
  private fun initializeSignCryptoKeys() {
    val adyenKey: String = /*Engine.getEngine7()
      .getClientPatchesJson()
      .getString(
        "adyenKey",*/
      "10001|C4F415A1A41A283417FAB7EF8580E077284BCC2B06F8A6C1785E31F5ABFD38A3E80760E0CA6437A8DC95BA4720A83203B99175889FA06FC6BABD4BF10EEEF0D73EF86DD336EBE68642AC15913B2FC24337BDEF52D2F5350224BD59F97C1B944BD03F0C3B4CA2E093A18507C349D68BE8BA54B458DB63D01377048F3E53C757F82B163A99A6A89AD0B969C0F745BB82DA7108B1D6FD74303711065B61009BC8011C27D1D1B5B9FC5378368F24DE03B582FE3490604F5803E805AEEA8B9EF86C54F27D9BD3FC4138B9DC30AF43A58CFF7C6ECEF68029C234BBC0816193DF9BD708D10AAFF6B10E38F0721CF422867C8CC5C554A357A8F51BA18153FB8A83CCBED1"
//      )
    val signParts = adyenKey.split("|").toTypedArray()
    val modulus = signParts[1]
    val publicExponent = signParts[0]
    val rsa = KeyFactory.getInstance("RSA")
    val spec = RSAPublicKeySpec(BigInteger(modulus, 16), BigInteger(publicExponent, 16))
    publicKey = rsa.generatePublic(spec)
    AESCmmNoPadding = Cipher.getInstance("AES/CCM/NoPadding", "BC")
    RSASignKeyPKCS1Padding = Cipher.getInstance("RSA/None/PKCS1Padding")
    RSASignKeyPKCS1Padding?.init(1, publicKey)
  }

  // $FF: renamed from: c (int) byte[]
  private fun generateIV(size: Int): ByteArray {
    val buff = ByteArray(size)
    secureRandom!!.nextBytes(buff)
    return buff
  }

  // $FF: renamed from: c (java.lang.String) java.lang.String
  @Throws(
    InvalidAlgorithmParameterException::class,
    InvalidKeyException::class,
    IllegalBlockSizeException::class,
    BadPaddingException::class
  )
  private fun encrypt(input: String): String {
    val secretKey1 = generateAESKey(256)
    val rndNums = generateIV(12)
    AESCmmNoPadding!!.init(1, secretKey1, IvParameterSpec(rndNums))
    val encryptedInput = AESCmmNoPadding!!.doFinal(input.toByteArray())
    val finalEncodedPayload = ByteArray(rndNums.size + encryptedInput.size)
    System.arraycopy(rndNums, 0, finalEncodedPayload, 0, rndNums.size)
    System.arraycopy(encryptedInput, 0, finalEncodedPayload, rndNums.size, encryptedInput.size)
    val encryptedSecret = RSASignKeyPKCS1Padding!!.doFinal(secretKey1.encoded)
    val params = arrayOfNulls<Any>(6)
    params[0] = "adyenjs_"
    params[1] = "0_1_12"
    params[2] = "$"
    params[3] = Base64.getEncoder().encodeToString(encryptedSecret)
    params[4] = "$"
    params[5] = Base64.getEncoder().encodeToString(finalEncodedPayload)
    return String.format("%s%s%s%s%s%s", *params)
  }

  companion object {
    // $FF: renamed from: c java.security.SecureRandom
    var secureRandom: SecureRandom? = null

    // $FF: renamed from: c java.text.SimpleDateFormat
    private val dateFormat: SimpleDateFormat

    // $FF: renamed from: c boolean
    var assertionsDisabled = false

    init {
//    f_String_7 = "adyenjs_"
//    f_String_6 = "1"
//    f_String_5 = "$"
//    f_String_4 = "0_1_12"
//    f_String_2 = "1"
//    f_String_1 = "10"
//    f_String_0 = "1"
//    f_String_c = "1"
      assertionsDisabled = !Adyen12::class.java.desiredAssertionStatus()
      secureRandom = SecureRandom()
      dateFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'")
//    f_String_3 = null
      Security.addProvider(BouncyCastleProvider())
      dateFormat.timeZone = TimeZone.getTimeZone("UTC")
    }
  }
}
