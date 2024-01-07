package services

type ProxyRotationPolicy string
const (
	ProxyRotationSwitch = "Switch"
	ProxyRotationSticky = "Sticky"
)
//
//type ProxyProvider interface {
//	GetBySetId(context context.Context, id string) ([]*url.URL, error)
//}
//
//func NewProxyProvider(conn *grpc.ClientConn) ProxyProvider {
//  client := taskmanager.NewProxyClient(conn)
//	return &proxyProvider{client: client}
//}
//
//type proxyProvider struct {
//  client taskmanager.ProxyClient
//}
//
//func (p *proxyProvider) GetBySetId(context context.Context, id string) ([]*url.URL, error) {
//  pool, err := p.client.GetPoolById(context, &common.ByIdRequest{Id: id})
//  if err != nil {
//    return nil, err
//  }
//
//  urls := make([]*url.URL, len(pool.Proxies))
//  for ix := range pool.Proxies {
//    proxy := pool.Proxies[ix]
//    proxyURL, err := util.GetProxyURL(proxy)
//    if err != nil {
//      log.Println("failed to parse proxy url " + proxy.Value)
//      continue
//    }
//
//    urls = append(urls, proxyURL)
//  }
//
//  return urls, nil
//}
