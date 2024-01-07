package walmart

type Account struct {
	Email    string
	Password string
}

type PaymentMethod struct {
	CardType string
}

type Profile struct {
	PostalCode    string
	City          string
	State         string
	AddressLine1  string
	AddressLine2  string
	FirstName     string
	LastName      string
	Phone         string
	Email         string
	ExpiryMonth   string
	ExpiryYear    string
	CardNumber    string
	Cvv           string
	PaymentMethod PaymentMethod
}

type PaymentToken struct {
	encryptedPan   string
	encryptedCvv   string
	integrityCheck string
	keyId          string
	phase          string
	piHash         string
}

type PaymentInstance struct {
	isFast__probably bool
	mobilegrief      bool
	mobilegriefalt   bool
}

func createPaymentToken(key, cardNumber, cvv string) *PaymentToken {
	panic("not implemented")
}

func encryptPan(key, cardNumber, cvv string) string {
	panic("not implemented")
}
