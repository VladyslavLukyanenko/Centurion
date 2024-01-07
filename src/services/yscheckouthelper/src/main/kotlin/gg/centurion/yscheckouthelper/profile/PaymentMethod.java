package gg.centurion.yscheckouthelper.profile;

public enum PaymentMethod {

    DISCOVER("DISCOVER"), MASTERCARD("MASTERCARD"), AMEX("AMEX"), VISA("VISA");

    public String method;

    public static PaymentMethod[] $VALUES;

    public String getFirstLetterUppercase() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        char char10000 = this.method.charAt(0);
        return char10000 + this.method.substring(1).toLowerCase();
    }

    public static PaymentMethod detectMethod(String string0) {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        switch(string0.charAt(0)) {
            case '3':
                return AMEX;
            case '4':
                return VISA;
            case '5':
            default:
                return MASTERCARD;
            case '6':
                return DISCOVER;
        }
    }

    static {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        PaymentMethod[] paymentMethod10000 = new PaymentMethod[4];
        paymentMethod10000[0] = VISA;
        paymentMethod10000[1] = AMEX;
        paymentMethod10000[2] = DISCOVER;
        paymentMethod10000[3] = MASTERCARD;
        $VALUES = paymentMethod10000;
    }

    public String get() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.method;
    }

    PaymentMethod(String string3) {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        // super(string1, int2);
        this.method = string3;
    }
}
