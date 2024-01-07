package util

import (
	"math/rand"
	"strings"
)

const rndStrSymbols = "abcdefghijklmnopqrstuvwxyz0123456789"

func RandInt(min, max int) int {
	return rand.Intn(max-min) + min
}

func GetInstanaID() string {
	b := strings.Builder{}
	for b.Len() < 16 {
		ix := rand.Intn(len(rndStrSymbols))
		b.WriteByte(rndStrSymbols[ix])
	}

	return b.String()
}
