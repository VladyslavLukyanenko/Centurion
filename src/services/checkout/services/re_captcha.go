package services

import (
	"context"
	"sync"
	"time"
)

const reCaptchaTokenLifetime = time.Second * 115

type CaptchaToken struct {
	cookieV3 *string
	solvedAt time.Time
}

func (t *CaptchaToken) GetCookieV3() *string {
	return t.cookieV3
}

func (t *CaptchaToken) IsExpired() bool {
	return t.solvedAt.Add(reCaptchaTokenLifetime).Unix() < time.Now().UTC().Unix()
}

func (t *CaptchaToken) Expire() {
	t.solvedAt = time.Unix(-1, -1)
}

type ReCaptchaSolver interface {
	Solve(ctx context.Context) (*CaptchaToken, error)
}

type ReCaptchaSolverProvider interface {
	Get(harvesterId, userId, productUrl string) ReCaptchaSolver
}

type reCaptchaSolverProvider struct {
	solvers *sync.Map

	// services
	rpc RpcManagerFactory
}

func NewReCaptchaSolverProvider(rpc RpcManagerFactory) ReCaptchaSolverProvider {
	return &reCaptchaSolverProvider{
		solvers: &sync.Map{},
		rpc:     rpc,
	}
}

func (r *reCaptchaSolverProvider) Get(harvesterId, userId, productUrl string) ReCaptchaSolver {
	key := harvesterId + "__" + userId + "__" + productUrl
	solver, _ := r.solvers.LoadOrStore(key, newReCaptchaSolver(harvesterId, userId, productUrl, r.rpc.Get(userId)))

	return solver.(*reCaptchaSolver)
}

type reCaptchaSolver struct {
	// state
	value       *CaptchaToken
	harvesterId string
	userId      string
	productUrl  string
	mu          *sync.Mutex

	// services
	rpc RpcManager
}

func newReCaptchaSolver(harvesterId, userId, productUrl string, rpc RpcManager) *reCaptchaSolver {
	return &reCaptchaSolver{
		harvesterId: harvesterId,
		userId:     userId,
		productUrl: productUrl,
		mu:         &sync.Mutex{},
		rpc:        rpc,
	}
}

func (r *reCaptchaSolver) Solve(ctx context.Context) (*CaptchaToken, error) {
	r.mu.Lock()
	defer r.mu.Unlock()
	if ctx.Err() != nil {
		return nil, ctx.Err()
	}

	if r.value == nil || r.value.IsExpired() {
		captcha, err := r.rpc.SolveCaptcha(r.userId, r.harvesterId, r.productUrl)
		if err != nil {
			return nil, err
		}

		r.value = &CaptchaToken{
			cookieV3: &captcha.CaptchaToken,
			solvedAt: captcha.SolvedAt.AsTime(),
		}
	}

	return r.value, nil
}
