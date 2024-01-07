package core

type ManagedProfile struct {
	id                string
	ProfileName       string
	Email             *string
	Phone             *string
	Shipping          *AddressData
	BillingAsShipping bool
	Billing           *AddressData
	Payment           *PaymentCard
}

func (m *ManagedProfile) GetId() string {
	return m.id
}

type AddressData struct {
	FirstName string
	LastName  string
	Country   string
	Address   string
	Address2  string
	State     string
	City      string
	Zipcode   string
}

type PaymentCard struct {
	CardName   string
	CardType   string
	CardNumber string
	CardMonth  string
	CardYear   string
	CardCvv    string
}

type ManagedProfileGroup struct {
	Name     string
	Profiles interface{}
}
