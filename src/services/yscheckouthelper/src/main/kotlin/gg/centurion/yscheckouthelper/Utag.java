package gg.centurion.yscheckouthelper;

import java.time.Instant;
import java.util.concurrent.ThreadLocalRandom;

public class Utag {

  public static int session_timeout;

  public int updateCookieCounter;

  public long ses_id;

  // $FF: renamed from: st long
  public long f_long_st;

  public long exp;

  public Object bmak;

  public String userAgent;

  public String productName;

  public String sku;

  public String documentUrl;

  public String v_id;

  public void updateDocumentUrl(String string1) {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    this.documentUrl = string1;
  }

  public String getPrevPage() {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    if (!this.documentUrl.contains("product") && !this.documentUrl.contains("archive")) {
      if (this.documentUrl.equals("QUEUE_URL")) {
        return this.getEncodedWR();
      } else if (this.documentUrl.contains("/delivery")) {
        return this.getEncodedShipping();
      } else {
        return this.documentUrl.contains("/payment") ? this.getEncodedProcessing() : "HOME";
      }
    } else {
      return this.getEncodedProductName();
    }
  }

  public void setName(String string1) {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    this.productName = string1.replace(" ", "%20");
  }

  public Utag(String string1, String string2) {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    // super();
    this.ses_id = (long) -1422559579 ^ 1422559578L;
    this.updateCookieCounter = 0;
    this.bmak = null;
    this.userAgent = string1;
    this.documentUrl = "https://www.yeezysupply.com/";
    this.sku = string2;
  }

  // $FF: renamed from: H () int
  public static int getIntH() {
    if (false) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    return (int) Math.ceil((double) Instant.now().toEpochMilli() / Double.longBitsToDouble(4725570615333879808L));
  }

  public String getEncodedProductName() {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    return "PRODUCT%7C" + this.productName + "%20(" + this.sku + ")";
  }

  static {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    session_timeout = 1800000;
  }
//
//  public Utag(Bmak bmak1, String string2) {
//    if (true) {
//      // while__INVOKEDYNAMIC__();
//      // fuck__INVOKEDYNAMIC__();
//    }
//    // super();
//    this.ses_id = (long) -360146390 ^ 360146389L;
//    this.updateCookieCounter = 0;
//    this.bmak = bmak1;
//    this.userAgent = null;
//    this.documentUrl = "https://www.yeezysupply.com/";
//    this.sku = string2;
//  }

  // $FF: renamed from: vi (long) String
  public String stringLongvi(long long1) {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    int int10002;
    String string3;
    int int10003;
    String string4;
    if (this.bmak != null && this.v_id == null) {
//      string3 = this.pad(long1.makeConcatWithConstants__INVOKEDYNAMIC__(long1), 12);
//      string4 = ThreadLocalRandom.current().nextDouble().makeConcatWithConstants__INVOKEDYNAMIC__(ThreadLocalRandom.current().nextDouble());
//      string3 = string3 + this.pad(string4.substring(2), 16);
//      string3 = string3 + this.pad(this.bmak.getDevice().getPluginLength().makeConcatWithConstants__INVOKEDYNAMIC__(this.bmak.getDevice().getPluginLength()), 2);
//      string3 = string3 + this.pad(this.bmak.getDevice().getUserAgent().length().makeConcatWithConstants__INVOKEDYNAMIC__(this.bmak.getDevice().getUserAgent().length()), 3);
//      string3 = string3 + this.pad(this.documentUrl.length().makeConcatWithConstants__INVOKEDYNAMIC__(this.documentUrl.length()), 4);
//      string3 = string3 + this.pad(this.bmak.getDevice().getUserAgent().replace("Mozilla/", "").length().makeConcatWithConstants__INVOKEDYNAMIC__(this.bmak.getDevice().getUserAgent().replace("Mozilla/", "").length()), 3);
//      int10002 = this.bmak.getDevice().getScreenWidth();
//      int10003 = -this.bmak.getDevice().getScreenHeight() + -1;
//      int10002 = (int10002 & ~int10003) - (int10003 & ~int10002) + -2 + 1;
//      int10003 = this.bmak.getDevice().getColorDepth();
//      string3 = string3 + this.pad(((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002)).makeConcatWithConstants__INVOKEDYNAMIC__((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002)), 5);
//      this.v_id = string3;
    } else if (this.userAgent != null && this.v_id == null) {
      var ts = this.pad(long1 + "", 12);
      var doubleRnd = ThreadLocalRandom.current().nextDouble() + "";
      var doubleRndStr = ts + this.pad(doubleRnd.substring(2), 16);
      var _3str = doubleRndStr + this.pad("3", 2);
      var ua = _3str + this.pad(this.userAgent.length() + "", 3);
      var doc = ua + this.pad(this.documentUrl.length() + "", 4);
      var ua2 = doc + this.pad(this.userAgent.length() + "", 3);
      int10002 = (int) Math.floor(Math.random() * Double.longBitsToDouble(4655459775352406016L));
      int10002 = ((800 + (int10002 & -801)) * 2 & ~((int10002 | 800) - (int10002 & 800))) - ((int10002 | 800) - (int10002 & 800) & ~((800 + (int10002 & -801)) * 2));
      int10003 = (int) Math.floor(Math.random() * Double.longBitsToDouble(4650608730050658304L));
      ua2 = ua2 + this.pad(((((600 + ((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & -601)) * 2 & ~(((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) | 600) & ~((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & 600))) - (((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) | 600) & ~((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & 600) & ~((600 + ((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & -601)) * 2)) & 24) * 2 - (((600 + ((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & -601)) * 2 & ~(((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) | 600) & ~((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & 600))) - (((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) | 600) & ~((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & 600) & ~((600 + ((int10003 + (int10002 & ~int10003) | (int10003 | ~int10002) - ~int10002) + (int10003 + (int10002 & ~int10003) & (int10003 | ~int10002) - ~int10002) & -601)) * 2)) ^ -25) + -2 + 1) + "", 5);
      this.v_id = ua2;
    }
    return this.v_id;
  }

  public String getEncodedProcessing() {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    return "CHECKOUT%7CPAYMENT";
  }

  public String getEncodedWR() {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    return "WAITING%20ROOM%7C" + this.productName + "%7C" + this.productName + "%20(" + this.sku + ")";
  }

  public String getEncodedShipping() {
    if (false) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    return "CHECKOUT%7CSHIPPING";
  }

  public String pad(String string1, int int2) {
    if (false) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    string1 = Long.toString(Long.parseLong(string1), 16) + "";
    String string3 = "";
    if (int2 > string1.length()) {
      int int4 = 0;
      while (true) {
        int int10002 = string1.length();
        if (int4 >= ((int2 | int10002) & (~int10002 | ~int2)) + ~((int10002 - (int2 & int10002)) * 2) + 1) {
          break;
        }
        string3 = "";
        ++int4;
      }
    }
    return string3 + string1;
  }

  // $FF: renamed from: r () String
  public static String getStringr() {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    String string0 = "0123456789";
    String string1 = "";
    String string2 = "";
    int int3 = 10;
    int int4 = 10;
    for (int int5 = 0; 19 > int5; ++int5) {
      int int6 = ThreadLocalRandom.current().nextInt(int3);
      string1 = "" + "0123456789".substring(int6, (1 + (int6 & -2) | (1 | ~int6) - ~int6) + (1 + (int6 & -2) & (1 | ~int6) - ~int6));
      if (0 == int5 && 9 == int6) {
        int3 = 3;
      } else if ((1 == int5 || 2 == int5) && 10 != int3 && 2 > int6) {
        int3 = 10;
      } else if (2 < int5) {
        int3 = 10;
      }
      int6 = ThreadLocalRandom.current().nextInt(int4);
      string2 = "" + "0123456789".substring(int6, (1 + (int6 & -2)) * 2 + ~(int6 & -2 | 1 & ~int6) + 1);
      if (0 == int5 && 9 == int6) {
        int4 = 3;
      } else if ((1 == int5 || 2 == int5) && 10 != int4 && 2 > int6) {
        int4 = 10;
      } else if (2 < int5) {
        int4 = 10;
      }
    }
    return string1 + string2;
  }

  public String genUtagMain() {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    this.stringLongvi(Instant.now().toEpochMilli());
    if (this.ses_id == ((long) 1954894624 ^ -1954894625L)) {
      this.ses_id = Instant.now().toEpochMilli();
    }
    this.f_long_st = Instant.now().toEpochMilli() + ((long) -531316754 ^ -531642194L);
    this.exp = Instant.now().toEpochMilli() + (long) ThreadLocalRandom.current().nextInt(470, 480) + ((long) 167262999 ^ 164550039L);
    int int10001 = this.updateCookieCounter;
    this.updateCookieCounter = (1 + (int10001 & -2) | (1 | ~int10001) - ~int10001) + (1 + (int10001 & -2) & (1 | ~int10001) - ~int10001);
    String string10000 = this.v_id;
    return "v_id:" + string10000 + "$_se:" + this.updateCookieCounter + "$_ss:" + (this.updateCookieCounter == 1 ? 1 : 0) + "$_st:" + this.f_long_st + "$ses_id:" + this.ses_id + "%3Bexp-session$_pn:" + this.updateCookieCounter + "%3Bexp-session$_prevpage:" + this.getPrevPage() + "%3Bexp-" + this.exp;
  }
}
