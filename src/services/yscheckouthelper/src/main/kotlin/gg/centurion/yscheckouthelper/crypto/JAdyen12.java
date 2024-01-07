package gg.centurion.yscheckouthelper.crypto;

import gg.centurion.contract.Profile;
import io.vertx.core.json.JsonObject;
import org.bouncycastle.jce.provider.BouncyCastleProvider;

import javax.crypto.*;
import javax.crypto.spec.IvParameterSpec;
import java.math.BigInteger;
import java.security.*;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.RSAPublicKeySpec;
import java.text.SimpleDateFormat;
import java.util.Base64;
import java.util.Date;
import java.util.TimeZone;

public class JAdyen12 {

  public static String initializeCount;

  public static String luhnSameLengthCount;

  public static String luhnCount;

  public static String PREFIX;

  public static String VERSION;

  public static String dfValue;

  public static String luhnOkCount;

  public static String sjclStrength;

  public static String SEPARATOR;

  public static PublicKey pubKey;

  public static SecureRandom srandom;

  public static SimpleDateFormat simpleDateFormat;

  public static Cipher aesCipher;

  public static Cipher rsaCipher;

  public static boolean $assertionsDisabled;

  public static SecretKey generateAESKey(int int0) throws NoSuchAlgorithmException {
    KeyGenerator keyGenerator1 = null;
    keyGenerator1 = KeyGenerator.getInstance("AES");
    if (!$assertionsDisabled && keyGenerator1 == null) {
      throw new AssertionError();
    } else {
      keyGenerator1.init(int0);
      return keyGenerator1.generateKey();
    }
  }

  public static void initRSA() throws NoSuchAlgorithmException, InvalidKeySpecException, NoSuchPaddingException, NoSuchProviderException, InvalidKeyException {
    String string0 = "10001|C4F415A1A41A283417FAB7EF8580E077284BCC2B06F8A6C1785E31F5ABFD38A3E80760E0CA6437A8DC95BA4720A83203B99175889FA06FC6BABD4BF10EEEF0D73EF86DD336EBE68642AC15913B2FC24337BDEF52D2F5350224BD59F97C1B944BD03F0C3B4CA2E093A18507C349D68BE8BA54B458DB63D01377048F3E53C757F82B163A99A6A89AD0B969C0F745BB82DA7108B1D6FD74303711065B61009BC8011C27D1D1B5B9FC5378368F24DE03B582FE3490604F5803E805AEEA8B9EF86C54F27D9BD3FC4138B9DC30AF43A58CFF7C6ECEF68029C234BBC0816193DF9BD708D10AAFF6B10E38F0721CF422867C8CC5C554A357A8F51BA18153FB8A83CCBED1";
    String[] string1 = string0.split("\\|");
    String string2 = string1[1];
    String string3 = string1[0];
    KeyFactory keyFactory4 = KeyFactory.getInstance("RSA");
    RSAPublicKeySpec rSAPublicKeySpec5 = new RSAPublicKeySpec(new BigInteger(string2, 16), new BigInteger(string3, 16));
    pubKey = keyFactory4.generatePublic(rSAPublicKeySpec5);
    aesCipher = Cipher.getInstance("AES/CCM/NoPadding", "BC");
    rsaCipher = Cipher.getInstance("RSA/None/PKCS1Padding");
    rsaCipher.init(1, pubKey);
  }

  public static String encryptAES(String string0) throws InvalidAlgorithmParameterException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException, NoSuchAlgorithmException {
    SecretKey secretKey1 = generateAESKey(256);
    byte[] byte2 = generateIV(12);
    aesCipher.init(1, secretKey1, new IvParameterSpec(byte2));
    byte[] byte3 = aesCipher.doFinal(string0.getBytes());
    byte[] finalEncodedPayload = new byte[(byte3.length + (byte2.length & ~byte3.length) ^ (byte3.length | ~byte2.length) - ~byte2.length) + (byte3.length + (byte2.length & ~byte3.length) & (byte3.length | ~byte2.length) - ~byte2.length) * 2];
    System.arraycopy(byte2, 0, finalEncodedPayload, 0, byte2.length);
    System.arraycopy(byte3, 0, finalEncodedPayload, byte2.length, byte3.length);
    byte[] encryptedSecret = rsaCipher.doFinal(secretKey1.getEncoded());
    Object[] object10001 = new Object[6];
    object10001[0] = "adyenjs_";
    object10001[1] = "0_1_12";
    object10001[2] = "$";
    object10001[3] = Base64.getEncoder().encodeToString(encryptedSecret);
    object10001[4] = "$";
    object10001[5] = Base64.getEncoder().encodeToString(finalEncodedPayload);
    return String.format("%s%s%s%s%s%s", object10001);
  }

  public static String getCSEToken(Profile.ProfileData profile0) throws NoSuchPaddingException, NoSuchAlgorithmException, InvalidKeySpecException, NoSuchProviderException, InvalidKeyException, InvalidAlgorithmParameterException, IllegalBlockSizeException, BadPaddingException {
    if (false) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    String profileJson = getCardJSON(profile0);
    System.out.println("ProfileJson: " + profileJson);
    initRSA();
    final var encrypted = encryptAES(profileJson);
    System.out.println("Encrypted: " + encrypted);
    return encrypted;
  }

  static {
    sjclStrength = "10";
    initializeCount = "1";
    luhnCount = "1";
    luhnSameLengthCount = "1";
    SEPARATOR = "$";
    PREFIX = "adyenjs_";
    luhnOkCount = "1";
    VERSION = "0_1_12";
    $assertionsDisabled = !Adyen12.class.desiredAssertionStatus();
    srandom = new SecureRandom();
    simpleDateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
    dfValue = null;
    Security.addProvider(new BouncyCastleProvider());
    simpleDateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));
  }

  public static synchronized byte[] generateIV(int int0) {
    if (false) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    byte[] byte1 = new byte[int0];
    srandom.nextBytes(byte1);
    return byte1;
  }

  public static String getCardJSON(Profile.ProfileData profile0) {
    JsonObject jsonObject1 = new JsonObject();
    jsonObject1.put("cvc", profile0.getBilling().getCvv());
    jsonObject1.put("dfValue", dfValue);
    jsonObject1.put("expiryMonth", Integer.toString(profile0.getBilling().getExpirationMonth()));
    jsonObject1.put("expiryYear", profile0.getBilling().getExpirationYear());
    jsonObject1.put("generationtime", simpleDateFormat.format(new Date()));
    String string10002 = profile0.getFirstName().substring(0, 1).toUpperCase();
    jsonObject1.put("holderName", string10002 + profile0.getFirstName().substring(1).toLowerCase() + " " + profile0.getLastName().substring(0, 1).toUpperCase() + profile0.getLastName().substring(1).toLowerCase());
    jsonObject1.put("initializeCount", "1");
    jsonObject1.put("luhnCount", "1");
    jsonObject1.put("luhnOkCount", "1");
    jsonObject1.put("luhnSameLengthCount", "1");
    jsonObject1.put("number", profile0.getBilling().getCardNumber());
    jsonObject1.put("sjclStrength", "10");
    return jsonObject1.toString();
  }
}
