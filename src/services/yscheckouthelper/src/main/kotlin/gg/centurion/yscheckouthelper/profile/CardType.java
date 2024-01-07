package gg.centurion.yscheckouthelper.profile;

import java.util.regex.Pattern;

public enum CardType {

    CHINA_UNION_PAY("^62[0-9]{14,17}$"),
    AMEX("^3[47][0-9]{13}$"),
    JCB("^(?:2131|1800|35\\d{3})\\d{11}$"),
    UNKNOWN(""),
    VISA("^4[0-9]{12}(?:[0-9]{3}){0,2}$"),
    DINERS_CLUB("^3(?:0[0-5]\\d|095|6\\d{0,2}|[89]\\d{2})\\d{12,15}$"),
    DISCOVER("^6(?:011|[45][0-9]{2})[0-9]{12}$"),
    MASTERCARD("^(?:5[1-5]|2(?!2([01]|20)|7(2[1-9]|3))[2-7])\\d{14}$");

    public Pattern pattern;

    public static CardType[] $VALUES;

    static {
        CardType[] cardType10000 = new CardType[8];
        cardType10000[0] = UNKNOWN;
        cardType10000[1] = VISA;
        cardType10000[2] = MASTERCARD;
        cardType10000[3] = AMEX;
        cardType10000[4] = DINERS_CLUB;
        cardType10000[5] = DISCOVER;
        cardType10000[6] = JCB;
        cardType10000[7] = CHINA_UNION_PAY;
        $VALUES = cardType10000;
    }

    CardType(String string3) {
        this.pattern = Pattern.compile(string3);
    }

    public static CardType detect(String string0) {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        CardType[] cardType1 = values();
        int int2 = cardType1.length;
        for (int int3 = 0; int3 < int2; ++int3) {
            CardType cardType4 = cardType1[int3];
            if (null != cardType4.pattern && cardType4.pattern.matcher(string0).matches()) {
                return cardType4;
            }
        }
        return UNKNOWN;
    }
}
