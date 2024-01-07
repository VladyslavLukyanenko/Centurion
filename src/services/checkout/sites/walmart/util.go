package walmart

import (
  http "github.com/useflyent/fhttp"
  "strconv"
  "time"
)

func generatePassword() string {
  return ""
}

func mustConvInt(val string) int32 {
  i32, err := strconv.Atoi(val)
  if err != nil {
    panic(err)
  }

  return int32(i32)
}

var (
  MPAS = createPseudoHeaders([]string{
    "m",
    "p",
    "a",
    "s",
  })

  MSPA = createPseudoHeaders([]string{
    "m",
    "s",
    "p",
    "a",
  })

  MASP = createPseudoHeaders([]string{
    "m",
    "a",
    "s",
    "p",
  })
)

type header struct {
  Name  string
  Value string
}

func createPseudoHeaders(tokens []string) []*header {
  headers := make([]*header, len(tokens), len(tokens))
  for ix, _ := range tokens {
    string5 := tokens[ix]
    int7 := -1

    if string5 == "a" {
      int7 = 2
    }

    if string5 == "m" {
      int7 = 0
    }

    if string5 == "p" {
      int7 = 1
    }

    if string5 == "s" {
      int7 = 3
    }

    switch int7 {
    case 0:
      headers = append(headers, &header{Name: ":method", Value: "DEFAULT_VALUE"})
      break
    case 1:
      headers = append(headers, &header{Name: ":path", Value: "DEFAULT_VALUE"})
      break
    case 2:
      headers = append(headers, &header{Name: ":authority", Value: "DEFAULT_VALUE"})
      break
    case 3:
      headers = append(headers, &header{Name: ":scheme", Value: "DEFAULT_VALUE"})
      break
    }
  }

  return headers
}

func getDateTimeNowWeb() *string {
  now := time.Now().UTC().Format("EEE, DD MMM YYYY hh:mm:ss 'GMT'")
  return &now
}


func utils_getStringc() string {
  panic("not implemented")
}

func (t *walmartTask) removeCookieByName(name string) {
  t.cookies.SetCookies(t.getRootCookieHost(), []*http.Cookie {
    { Name: name, Expires: time.Now().Add(-1)},
  })
}