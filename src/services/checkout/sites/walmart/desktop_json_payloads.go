package walmart

import (
  "strings"
)

func (t *walmartTask) getJsonObject1() interface{} {
	return map[string]string{}
}

func (t *walmartTask) getJsonObjectb() interface{} {
	switch t.taskId {
	case 0:
		return t.jsonObjectStringInt0(t.keywords[0], mustConvInt(t.offerId))
	case 1:
		return t.getPreloadCartJson(t.keywords[0], mustConvInt(t.offerId))
	default:
		return t.getPreloadCartJson(t.keywords[0], mustConvInt(t.offerId))
	}
}

func (t *walmartTask) getJsonObjectl() interface{} {
	return map[string]interface{}{
		"0": map[string]interface{}{
			"tags": "[\"info\",\"home-app, scus-prod-a6, PROD\"]",
			"data": map[string]interface{}{
				"_type": "fetch",
				"extras": map[string]interface{}{
					"response": map[string]interface{}{
						"status": nil,
					},
				},
			},
		},
		"1": map[string]interface{}{
			"tags": "[\"info\",\"home-app, scus-prod-a6, PROD\"]",
			"data": map[string]interface{}{
				"_type": "fetch",
				"extras": map[string]interface{}{
					"response": map[string]interface{}{
						"status": nil,
					},
				},
			},
		},
	}
}

func (t *walmartTask) jsonObjectStringInt0(offerId string, quantity int32) interface{} {
	return map[string]interface{}{
		"offerId":  offerId,
		"quantity": quantity,
		"storeIds": t.storeIds,
		"location": map[string]interface{}{
			"postalCode":   mustConvInt(t.profile.PostalCode),
			"city":         t.profile.City,
			"state":        t.profile.State,
			"isZipLocated": t.isZipLocated,
		},
		"shipMethodDefaultRule": "SHIP_RULE_1",
	}
}

func (t *walmartTask) getPreloadCartJson(string1 string, quantity int32) interface{} {
	t.isZipLocated = strings.Contains(t.getCookiesStr(), t.profile.PostalCode)
	return map[string]interface{}{
		"quantity":   quantity,
		"actionType": "INCREASE",
		"customAttributes": []interface{}{
			map[string]interface{}{
				"type":  "NON_DISPLAY",
				"name":  "pita",
				"value": 0,
			},
		},
		"location": map[string]interface{}{
			"postalCode":   mustConvInt(t.profile.PostalCode),
			"city":         t.profile.City,
			"state":        t.profile.State,
			"isZipLocated": t.isZipLocated,
		},
		"storeIds": t.storeIds,
		"offerId":  string1,
	}
}

