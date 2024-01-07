package pixel

type GaneshAPI struct {
}

func (a *GaneshAPI) SendPixelData(pixelId string, tval string, scriptVal string) (string, error) {
  panic("implement me")
}

func (a *GaneshAPI) FetchAkamaiSensor(abck string, prodId string) (string, error) {
  panic("not implemented")
}

func (a *GaneshAPI) GetUserAgent() (string, error) {
  panic("not implemented")
}