package pixel

import (
  "context"
  "encoding/base64"
	"errors"
	"regexp"
	"strconv"
	"strings"
)

var (
	bazaPattern   = regexp.MustCompile("baz[A-z]*=?\"([0-9]*?)\"")
	akamPattern   = regexp.MustCompile("(https://www.yeezysupply.com/akam.*?)\"")
	hexarrPatterm = regexp.MustCompile("=\\[(\".*\\\\x.*?)\\];")
	gIndexPatterm = regexp.MustCompile("g=_\\[([0-9]*?)]")
	tvalPatterm   = regexp.MustCompile("t=([0-z]*)")
)

type PixelAPI interface {
  SendPixelData(ctx context.Context, pixelId string, tval string, scriptVal string) (string, error)
  GetUserAgent() (string, error)
}

func ExtractBaza(source string) (string, error) {
	matches := bazaPattern.FindAllStringSubmatch(source, 1)
	if matches == nil || len(matches) == 0 {
		return "", errors.New("No BAZA found")
	}

	return matches[0][1], nil
}

func ExtractAkam(source string) ([]string, error) {
	matches := akamPattern.FindAllStringSubmatch(source, -1)
	res := []string{}
	for _, v := range matches {
		res = append(res, v[1])
	}

	if len(res) < 2 {
		return nil, errors.New("No Akamai urls found")
	}

	return res, nil
}

func GetHexArr(html string) ([]string, error) {
	matches := hexarrPatterm.FindAllStringSubmatch(html, 1)
	if matches != nil && len(matches) > 0 {
		return decodeHexEncodedStrLines(strings.Split(matches[0][1], ","))
	}

	return nil, errors.New("No hex array")
}

func ParseGIndex(html string) (int, error) {
	matches := gIndexPatterm.FindAllStringSubmatch(html, 1)
	if matches != nil && len(matches) > 0 {
		return strconv.Atoi(matches[0][1])
	}

	return -1, errors.New("No G index found")
}

func ParseTVal(akamUrl string) (string, error) {
	token := strings.Split(akamUrl, "?")[1]
	eqIx := strings.Index(token, "=")
	valBase64 := token[eqIx+1:]
	if strings.Contains(valBase64, "&") {
		valBase64 = strings.Split(valBase64, "&")[0]
	}

	decodedBytes, err := base64.StdEncoding.DecodeString(valBase64)
	if err != nil {
		return "", err
	}

	matches := tvalPatterm.FindAllStringSubmatch(string(decodedBytes), -1)
	if matches != nil && len(matches) > 0 {
		return matches[0][1], nil
	}

	return "", errors.New("No T val found")
}

func decodeHexEncodedStrLines(lines []string) ([]string, error) {
	parsedLines := make([]string, len(lines), len(lines))
	for ix, _ := range lines {
		decoded, err := decodeHexEncodedStr(lines[ix])
		if err != nil {
			return nil, err
		}

		parsedLines[ix] = decoded
	}

	return parsedLines, nil
}

func decodeHexEncodedStr(hexNumber string) (string, error) {
	normalizedHex := strings.Replace(hexNumber, "\\x", "", -1)
	normalizedHex = strings.Replace(normalizedHex, "\"", "", -1)

	var bytes []byte
	for currIx := 0; currIx < len(normalizedHex); currIx = currIx + 2 {
		hex := normalizedHex[currIx : currIx+2]
		v, err := strconv.ParseUint(hex, 16, 0)
		if err != nil {
			return "", err
		}

		bytes = append(bytes, byte(v))
	}

	return string(bytes), nil
}
