package pixel

import (
	"context"
	"errors"
	"github.com/CenturionLabs/centurion/checkout-service/services"
	"github.com/CenturionLabs/centurion/checkout-service/util"
	jsoniter "github.com/json-iterator/go"
	http "github.com/useflyent/fhttp"
	"io/ioutil"
	"strings"
	"time"
)

const hawkApiKey = "ce3cabed-e10f-43b1-a2a2-8d2a9c2a212f"

type HawkAPI struct {
	ua   string
	http services.HttpClient
}

func NewHawkAPI(http services.HttpClient) PixelAPI {
	return &HawkAPI{http: http}
}

func (a *HawkAPI) SendPixelData(ctx context.Context, pixelId string, tval string, scriptVal string) (string, error) {
	for ix := 0; ix < 1000 && ctx.Err() == nil; ix++ {
		if ix != 0 {
			time.Sleep(time.Millisecond * time.Duration(util.RandInt(10_000, 15_000)))
		}

		data := a.scriptIdScriptSecretToJson(pixelId, scriptVal)
		json, err := jsoniter.MarshalToString(data)
		if err != nil {
			return "", err
		}

		req, err := a.createPostPixelRequest(json)
		if err != nil {
			continue
		}

		resp, err := a.http.Do(req)
		if err != nil {
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			continue
		}

		return string(b), nil
	}

	return "", errors.New("failed to send pixel data")
}

func (a *HawkAPI) GenAkamaiSensorData(abck string, prodId string, isUpperBound bool) (string, error) {
	for att := 0; att <= 1000; att++ {
		if att != 0 {
			time.Sleep(time.Millisecond * time.Duration(util.RandInt(10_000, 15_000)))
		}

		data := a.createGenAkamaiJson(abck, prodId, isUpperBound)
		json, err := jsoniter.MarshalToString(data)
		if err != nil {
			continue
		}

		req, err := a.createGenerateAkamaiRequest(json)
		if err != nil {
			continue
		}

		resp, err := a.http.Do(req)
		if err != nil {
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			continue
		}

		rawAkamai := string(b)
		akam := strings.Split(rawAkamai, "*")[0]
		return akam, nil
	}

	return "", errors.New("failed to generate sensor data")
}

func (a *HawkAPI) GetUserAgent() (string, error) {
	//return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36", nil
	req := a.createAkamaiUserAgentRequest()
	resp, err := a.http.Do(req)
	if err != nil {
		return "", err
	}

	b, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	return string(b), nil
	//return "Mozilla/5.0 (Linux; Android 11; SM-A102U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.72 Mobile Safari/537.36", nil
}

func (a *HawkAPI) createAkamaiUserAgentRequest() *http.Request {
	req := &http.Request{
		URL: util.MustParseUrl("https://ak01-eu.hwkapi.com/akamai/ua"),
		Header: http.Header{
			"X-Sec":     {"high"},
			"X-Api-Key": {hawkApiKey},
		},
	}

	return req
}

func (a *HawkAPI) createPostPixelRequest(json string) (*http.Request, error) {
	r, err := http.NewRequest("POST", "https://ak01-eu.hwkapi.com/akamai/pixel", strings.NewReader(json))
	if err != nil {
		return nil, err
	}

	util.AddHeaders(r.Header, http.Header{
		"X-Api-Key":    {hawkApiKey},
		"X-Sec":        {"high"},
		"Content-Type": {"application/json"},
	})

	return r, nil
}

func (a *HawkAPI) scriptIdScriptSecretToJson(pixelId string, scriptVal string) interface{} {
	return map[string]interface{}{
		"user_agent":    a.ua,
		"script_id":     pixelId,
		"script_secret": scriptVal,
	}
}

func (a *HawkAPI) createGenerateAkamaiRequest(json string) (*http.Request, error) {
	r, err := http.NewRequest("POST", "https://ak01-eu.hwkapi.com/akamai/generate", strings.NewReader(json))
	if err != nil {
		return nil, err
	}

	util.AddHeaders(r.Header, http.Header{
		"X-Api-Key":    {hawkApiKey},
		"X-Sec":        {"high"},
		"Content-Type": {"application/json"},
	})

	return r, nil
}

func (a *HawkAPI) createGenAkamaiJson(abck string, prodId string, isUpperBound bool) interface{} {
	events := "1,1"
	if !isUpperBound {
		events = "0,0"
	}

	return map[string]interface{}{
		"site":       "https://www.yeezysupply.com/product/" + prodId,
		"abck":       abck,
		"type":       "sensor",
		"events":     events,
		"user_agent": a.ua,
	}
}
