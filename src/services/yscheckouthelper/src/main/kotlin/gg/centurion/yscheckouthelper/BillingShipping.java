package gg.centurion.yscheckouthelper;


import gg.centurion.yscheckouthelper.profile.Profile;

public class BillingShipping {
  public static String createJson(Profile profile) {
    return "{\"customer\":{\"email\":\"" + profile.getEmail() + "\",\"receiveSmsUpdates\":false},\"shippingAddress\":{\"country\":\"US\",\"firstName\":\"" + profile.getFirstName().substring(0, 1).toUpperCase() + profile.getFirstName().substring(1).toLowerCase() + "\",\"lastName\":\"" + profile.getLastName().substring(0, 1).toUpperCase() + profile.getLastName().substring(1).toLowerCase() + "\",\"address1\":\"" + profile.getAddress1() + "\",\"address2\":\"" + profile.getAddress2() + "\",\"city\":\"" + profile.getCity().substring(0, 1).toUpperCase() + profile.getCity().substring(1).toLowerCase() + "\",\"stateCode\":\"" + profile.getState() + "\",\"zipcode\":\"" + profile.getZip() + "\",\"phoneNumber\":\"" + profile.getPhone() + "\"},\"billingAddress\":{\"country\":\"US\",\"firstName\":\"" + profile.getFirstName().substring(0, 1).toUpperCase() + profile.getFirstName().substring(1).toLowerCase() + "\",\"lastName\":\"" + profile.getLastName().substring(0, 1).toUpperCase() + profile.getLastName().substring(1).toLowerCase() + "\",\"address1\":\"" + profile.getAddress1() + "\",\"address2\":\"" + profile.getAddress2() + "\",\"city\":\"" + profile.getCity().substring(0, 1).toUpperCase() + profile.getCity().substring(1).toLowerCase() + "\",\"stateCode\":\"" + profile.getState() + "\",\"zipcode\":\"" + profile.getZip() + "\",\"phoneNumber\":\"" + profile.getPhone() + "\"},\"methodList\":[{\"id\":\"2ndDay-1\",\"shipmentId\":\"me\",\"collectionPeriod\":\"\",\"deliveryPeriod\":\"\"}],\"newsletterSubscription\":true}";
  }
}
