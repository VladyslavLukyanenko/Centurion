package gg.centurion.yscheckouthelper.profile;


import gg.centurion.yscheckouthelper.profile.constants.Countries;
import gg.centurion.yscheckouthelper.profile.constants.States;

public class Profile {

    public CardType cardType;

    public PaymentMethod paymentMethod;

    public String email;

    public String expiryMonth;

    public String accountPassword;

    public String firstName;

    public String fullCountry;

    public String city;

    public String zip;

    public String state;

    public String phone;

    public String address1;

    public String expiryYear;

    public String country;

    public String address2;

    public String lastName;

    public String accountEmail;

    public String cvv;

    public String fullState;

    public String cardNumber;

    public String getCardNumber() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.cardNumber;
    }

    public void setAccountPassword(String string1) {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        this.accountPassword = string1;
    }

    public PaymentMethod getPaymentMethod() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.paymentMethod;
    }

    public String getAccountEmail() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.accountEmail;
    }

    public String getExpiryMonth() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.expiryMonth;
    }

    public String getCity() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.city;
    }

    public String getExpiryYear() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.expiryYear;
    }

    public String getFullCountry() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.fullCountry;
    }

    public String getAccountPassword() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.accountPassword;
    }

    public Profile copy() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return new Profile(this);
    }

    public void setAccountEmail(String string1) {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        this.accountEmail = string1;
    }

    public String getLastName() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.lastName;
    }

    public String getAddress2() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.address2;
    }

    public String getEmail() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.accountEmail == null ? this.email : this.accountEmail;
    }

    public String getState() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.state;
    }

    public String getCountry() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.country;
    }

    public Profile(String[] string1) {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        // super();
        this.accountEmail = null;
        this.accountPassword = null;
        this.firstName = string1[2];
        this.lastName = string1[3];
        this.email = string1[4];
        this.phone = string1[5].replace("-", "").replace(" ", "").trim();
        this.address1 = string1[6];
        this.address2 = string1[7];
        this.state = string1[8].toUpperCase();
        String string10001 = string1[9].substring(0, 1).toUpperCase();
        this.city = string10001 + string1[9].substring(1).toLowerCase();
        this.zip = string1[10];
        this.country = string1[11].replace("USA", "US").replace("JAPAN", "JP").replace("CANADA", "CA");
        this.cardNumber = string1[12].replace("-", "").replace(" ", "").trim();
        if (string1[13].length() == 1) {
            this.expiryMonth = "0" + string1[13];
        } else {
            this.expiryMonth = string1[13];
        }
        if (string1[14].length() == 2) {
            this.expiryYear = "20" + string1[14];
        } else {
            this.expiryYear = string1[14];
        }
        this.cvv = string1[15];
        this.paymentMethod = PaymentMethod.detectMethod(this.cardNumber);
        this.cardType = CardType.detect(this.cardNumber);
        this.fullState = States.fullStateName(this.state);
        if (this.fullState == null) {
            System.out.println("Please check your state/prefecture -> " + this.state);
        }
        this.fullCountry = Countries.fullCountryName(this.country);
    }

    public String getFullState() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.fullState;
    }

    public String getAddress1() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.address1;
    }

    public String splitCard() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        String string1 = this.getCardNumber();
        String string2 = " ";
        int int10002 = string1.length();
        int int10003 = " ".length() * (string1.length() / (4));
        StringBuilder stringBuilder3 = new StringBuilder(((((int10003 + (int10002 & ~int10003)) * 2 & ~((int10002 | int10003) & (~int10003 | ~int10002))) * 2 - ((int10003 + (int10002 & ~int10003)) * 2 ^ (int10002 | int10003) & (~int10003 | ~int10002)) ^ -2) - (-2 & ~(((int10003 + (int10002 & ~int10003)) * 2 & ~((int10002 | int10003) & (~int10003 | ~int10002))) * 2 - ((int10003 + (int10002 & ~int10003)) * 2 ^ (int10002 | int10003) & (~int10003 | ~int10002)))) * 2 ^ 1) - (1 & ~((((int10003 + (int10002 & ~int10003)) * 2 & ~((int10002 | int10003) & (~int10003 | ~int10002))) * 2 - ((int10003 + (int10002 & ~int10003)) * 2 ^ (int10002 | int10003) & (~int10003 | ~int10002)) ^ -2) - (-2 & ~(((int10003 + (int10002 & ~int10003)) * 2 & ~((int10002 | int10003) & (~int10003 | ~int10002))) * 2 - ((int10003 + (int10002 & ~int10003)) * 2 ^ (int10002 | int10003) & (~int10003 | ~int10002)))) * 2)) * 2);
        int int4 = 0;
        for (String string5 = ""; int4 < string1.length(); int4 += 4) {
            stringBuilder3.append(string5);
            string5 = string2;
            stringBuilder3.append(string1, int4, Math.min(((int4 | 4) & ~(int4 & 4)) - ~(((4 | ~int4) - ~int4) * 2) - 1, string1.length()));
        }
        return stringBuilder3.toString();
    }

    public String toString() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return "Profile{firstName='" + this.firstName + "', lastName='" + this.lastName + "', email='" + this.email + "', phone='" + this.phone + "', address1='" + this.address1 + "', address2='" + this.address2 + "', state='" + this.state + "', fullState='" + this.fullState + "', city='" + this.city + "', country='" + this.country + "', fullCountry='" + this.fullCountry + "', zip='" + this.zip + "', paymentMethod=" + this.paymentMethod + ", cardType=" + this.cardType + ", cardNumber='" + this.cardNumber + "', expiryYear='" + this.expiryYear + "', expiryMonth='" + this.expiryMonth + "', cvv='" + this.cvv + "', accountEmail='" + this.accountEmail + "'}";
    }

    public String getFirstName() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.firstName;
    }

    public String getZip() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.zip;
    }

    public CardType getCardType() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.cardType;
    }

    public String getCvv() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.cvv;
    }

    public String getPhone() {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        return this.phone;
    }

    public String getLastDigits() {
        if (false) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        if (this.cardNumber.length() > 4) {
            String string10000 = this.cardNumber;
            int int10001 = this.cardNumber.length();
            return string10000.substring(((int10001 & -5 | 4 & ~int10001) & ~(((4 | int10001) - int10001) * 2)) - (((4 | int10001) - int10001) * 2 & ~(int10001 & -5 | 4 & ~int10001)));
        } else {
            return this.cardNumber;
        }
    }

    public Profile() {}
    public Profile(Profile profile1) {
        if (true) {
            // while__INVOKEDYNAMIC__();
            // fuck__INVOKEDYNAMIC__();
        }
        // super();
        this.accountEmail = null;
        this.accountPassword = null;
        this.firstName = profile1.firstName;
        this.lastName = profile1.lastName;
        this.email = profile1.email;
        this.phone = profile1.phone;
        this.address1 = profile1.address1;
        this.address2 = profile1.address2;
        this.state = profile1.state;
        this.fullState = profile1.fullState;
        this.city = profile1.city;
        this.country = profile1.country;
        this.fullCountry = profile1.fullCountry;
        this.zip = profile1.zip;
        this.paymentMethod = profile1.paymentMethod;
        this.cardType = profile1.cardType;
        this.cardNumber = profile1.cardNumber;
        this.expiryYear = profile1.expiryYear;
        this.expiryMonth = profile1.expiryMonth;
        this.cvv = profile1.cvv;
        this.accountEmail = profile1.accountEmail;
    }
}
