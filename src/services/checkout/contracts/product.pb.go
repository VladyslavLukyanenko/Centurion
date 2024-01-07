// Code generated by protoc-gen-go. DO NOT EDIT.
// versions:
// 	protoc-gen-go v1.26.0
// 	protoc        v3.15.5
// source: product.proto

package contracts

import (
	protoreflect "google.golang.org/protobuf/reflect/protoreflect"
	protoimpl "google.golang.org/protobuf/runtime/protoimpl"
	reflect "reflect"
	sync "sync"
)

const (
	// Verify that this generated code is sufficiently up-to-date.
	_ = protoimpl.EnforceVersion(20 - protoimpl.MinVersion)
	// Verify that runtime/protoimpl is sufficiently up-to-date.
	_ = protoimpl.EnforceVersion(protoimpl.MaxVersion - 20)
)

type FetchProductCommand struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Module  Module       `protobuf:"varint,1,opt,name=module,proto3,enum=Module" json:"module,omitempty"`
	Sku     string       `protobuf:"bytes,2,opt,name=sku,proto3" json:"sku,omitempty"`
	Proxies []*ProxyData `protobuf:"bytes,3,rep,name=proxies,proto3" json:"proxies,omitempty"`
}

func (x *FetchProductCommand) Reset() {
	*x = FetchProductCommand{}
	if protoimpl.UnsafeEnabled {
		mi := &file_product_proto_msgTypes[0]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *FetchProductCommand) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*FetchProductCommand) ProtoMessage() {}

func (x *FetchProductCommand) ProtoReflect() protoreflect.Message {
	mi := &file_product_proto_msgTypes[0]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use FetchProductCommand.ProtoReflect.Descriptor instead.
func (*FetchProductCommand) Descriptor() ([]byte, []int) {
	return file_product_proto_rawDescGZIP(), []int{0}
}

func (x *FetchProductCommand) GetModule() Module {
	if x != nil {
		return x.Module
	}
	return Module_YEEZY_SUPPLY
}

func (x *FetchProductCommand) GetSku() string {
	if x != nil {
		return x.Sku
	}
	return ""
}

func (x *FetchProductCommand) GetProxies() []*ProxyData {
	if x != nil {
		return x.Proxies
	}
	return nil
}

type ProductData struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Sku    string   `protobuf:"bytes,1,opt,name=sku,proto3" json:"sku,omitempty"`
	Name   string   `protobuf:"bytes,2,opt,name=name,proto3" json:"name,omitempty"`
	Image  string   `protobuf:"bytes,3,opt,name=image,proto3" json:"image,omitempty"`
	Link   string   `protobuf:"bytes,4,opt,name=link,proto3" json:"link,omitempty"`
	Module Module   `protobuf:"varint,5,opt,name=module,proto3,enum=Module" json:"module,omitempty"`
	Price  *float64 `protobuf:"fixed64,6,opt,name=price,proto3,oneof" json:"price,omitempty"`
}

func (x *ProductData) Reset() {
	*x = ProductData{}
	if protoimpl.UnsafeEnabled {
		mi := &file_product_proto_msgTypes[1]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *ProductData) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*ProductData) ProtoMessage() {}

func (x *ProductData) ProtoReflect() protoreflect.Message {
	mi := &file_product_proto_msgTypes[1]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use ProductData.ProtoReflect.Descriptor instead.
func (*ProductData) Descriptor() ([]byte, []int) {
	return file_product_proto_rawDescGZIP(), []int{1}
}

func (x *ProductData) GetSku() string {
	if x != nil {
		return x.Sku
	}
	return ""
}

func (x *ProductData) GetName() string {
	if x != nil {
		return x.Name
	}
	return ""
}

func (x *ProductData) GetImage() string {
	if x != nil {
		return x.Image
	}
	return ""
}

func (x *ProductData) GetLink() string {
	if x != nil {
		return x.Link
	}
	return ""
}

func (x *ProductData) GetModule() Module {
	if x != nil {
		return x.Module
	}
	return Module_YEEZY_SUPPLY
}

func (x *ProductData) GetPrice() float64 {
	if x != nil && x.Price != nil {
		return *x.Price
	}
	return 0
}

var File_product_proto protoreflect.FileDescriptor

var file_product_proto_rawDesc = []byte{
	0x0a, 0x0d, 0x70, 0x72, 0x6f, 0x64, 0x75, 0x63, 0x74, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x1a,
	0x0c, 0x6d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x1a, 0x0b, 0x70,
	0x72, 0x6f, 0x78, 0x79, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x22, 0x6e, 0x0a, 0x13, 0x46, 0x65,
	0x74, 0x63, 0x68, 0x50, 0x72, 0x6f, 0x64, 0x75, 0x63, 0x74, 0x43, 0x6f, 0x6d, 0x6d, 0x61, 0x6e,
	0x64, 0x12, 0x1f, 0x0a, 0x06, 0x6d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x18, 0x01, 0x20, 0x01, 0x28,
	0x0e, 0x32, 0x07, 0x2e, 0x4d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x52, 0x06, 0x6d, 0x6f, 0x64, 0x75,
	0x6c, 0x65, 0x12, 0x10, 0x0a, 0x03, 0x73, 0x6b, 0x75, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52,
	0x03, 0x73, 0x6b, 0x75, 0x12, 0x24, 0x0a, 0x07, 0x70, 0x72, 0x6f, 0x78, 0x69, 0x65, 0x73, 0x18,
	0x03, 0x20, 0x03, 0x28, 0x0b, 0x32, 0x0a, 0x2e, 0x50, 0x72, 0x6f, 0x78, 0x79, 0x44, 0x61, 0x74,
	0x61, 0x52, 0x07, 0x70, 0x72, 0x6f, 0x78, 0x69, 0x65, 0x73, 0x22, 0xa3, 0x01, 0x0a, 0x0b, 0x50,
	0x72, 0x6f, 0x64, 0x75, 0x63, 0x74, 0x44, 0x61, 0x74, 0x61, 0x12, 0x10, 0x0a, 0x03, 0x73, 0x6b,
	0x75, 0x18, 0x01, 0x20, 0x01, 0x28, 0x09, 0x52, 0x03, 0x73, 0x6b, 0x75, 0x12, 0x12, 0x0a, 0x04,
	0x6e, 0x61, 0x6d, 0x65, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52, 0x04, 0x6e, 0x61, 0x6d, 0x65,
	0x12, 0x14, 0x0a, 0x05, 0x69, 0x6d, 0x61, 0x67, 0x65, 0x18, 0x03, 0x20, 0x01, 0x28, 0x09, 0x52,
	0x05, 0x69, 0x6d, 0x61, 0x67, 0x65, 0x12, 0x12, 0x0a, 0x04, 0x6c, 0x69, 0x6e, 0x6b, 0x18, 0x04,
	0x20, 0x01, 0x28, 0x09, 0x52, 0x04, 0x6c, 0x69, 0x6e, 0x6b, 0x12, 0x1f, 0x0a, 0x06, 0x6d, 0x6f,
	0x64, 0x75, 0x6c, 0x65, 0x18, 0x05, 0x20, 0x01, 0x28, 0x0e, 0x32, 0x07, 0x2e, 0x4d, 0x6f, 0x64,
	0x75, 0x6c, 0x65, 0x52, 0x06, 0x6d, 0x6f, 0x64, 0x75, 0x6c, 0x65, 0x12, 0x19, 0x0a, 0x05, 0x70,
	0x72, 0x69, 0x63, 0x65, 0x18, 0x06, 0x20, 0x01, 0x28, 0x01, 0x48, 0x00, 0x52, 0x05, 0x70, 0x72,
	0x69, 0x63, 0x65, 0x88, 0x01, 0x01, 0x42, 0x08, 0x0a, 0x06, 0x5f, 0x70, 0x72, 0x69, 0x63, 0x65,
	0x42, 0x6c, 0x0a, 0x15, 0x67, 0x67, 0x2e, 0x63, 0x65, 0x6e, 0x74, 0x75, 0x72, 0x69, 0x6f, 0x6e,
	0x2e, 0x63, 0x6f, 0x6e, 0x74, 0x72, 0x61, 0x63, 0x74, 0x5a, 0x3d, 0x67, 0x69, 0x74, 0x68, 0x75,
	0x62, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x43, 0x65, 0x6e, 0x74, 0x75, 0x72, 0x69, 0x6f, 0x6e, 0x4c,
	0x61, 0x62, 0x73, 0x2f, 0x63, 0x65, 0x6e, 0x74, 0x75, 0x72, 0x69, 0x6f, 0x6e, 0x2f, 0x63, 0x68,
	0x65, 0x63, 0x6b, 0x6f, 0x75, 0x74, 0x2d, 0x73, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x2f, 0x63,
	0x6f, 0x6e, 0x74, 0x72, 0x61, 0x63, 0x74, 0x73, 0xaa, 0x02, 0x13, 0x43, 0x65, 0x6e, 0x74, 0x75,
	0x72, 0x69, 0x6f, 0x6e, 0x2e, 0x43, 0x6f, 0x6e, 0x74, 0x72, 0x61, 0x63, 0x74, 0x73, 0x62, 0x06,
	0x70, 0x72, 0x6f, 0x74, 0x6f, 0x33,
}

var (
	file_product_proto_rawDescOnce sync.Once
	file_product_proto_rawDescData = file_product_proto_rawDesc
)

func file_product_proto_rawDescGZIP() []byte {
	file_product_proto_rawDescOnce.Do(func() {
		file_product_proto_rawDescData = protoimpl.X.CompressGZIP(file_product_proto_rawDescData)
	})
	return file_product_proto_rawDescData
}

var file_product_proto_msgTypes = make([]protoimpl.MessageInfo, 2)
var file_product_proto_goTypes = []interface{}{
	(*FetchProductCommand)(nil), // 0: FetchProductCommand
	(*ProductData)(nil),         // 1: ProductData
	(Module)(0),                 // 2: Module
	(*ProxyData)(nil),           // 3: ProxyData
}
var file_product_proto_depIdxs = []int32{
	2, // 0: FetchProductCommand.module:type_name -> Module
	3, // 1: FetchProductCommand.proxies:type_name -> ProxyData
	2, // 2: ProductData.module:type_name -> Module
	3, // [3:3] is the sub-list for method output_type
	3, // [3:3] is the sub-list for method input_type
	3, // [3:3] is the sub-list for extension type_name
	3, // [3:3] is the sub-list for extension extendee
	0, // [0:3] is the sub-list for field type_name
}

func init() { file_product_proto_init() }
func file_product_proto_init() {
	if File_product_proto != nil {
		return
	}
	file_module_proto_init()
	file_proxy_proto_init()
	if !protoimpl.UnsafeEnabled {
		file_product_proto_msgTypes[0].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*FetchProductCommand); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_product_proto_msgTypes[1].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*ProductData); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
	}
	file_product_proto_msgTypes[1].OneofWrappers = []interface{}{}
	type x struct{}
	out := protoimpl.TypeBuilder{
		File: protoimpl.DescBuilder{
			GoPackagePath: reflect.TypeOf(x{}).PkgPath(),
			RawDescriptor: file_product_proto_rawDesc,
			NumEnums:      0,
			NumMessages:   2,
			NumExtensions: 0,
			NumServices:   0,
		},
		GoTypes:           file_product_proto_goTypes,
		DependencyIndexes: file_product_proto_depIdxs,
		MessageInfos:      file_product_proto_msgTypes,
	}.Build()
	File_product_proto = out.File
	file_product_proto_rawDesc = nil
	file_product_proto_goTypes = nil
	file_product_proto_depIdxs = nil
}