package services

import (
	contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/module_reflection"
	descriptor2 "github.com/golang/protobuf/descriptor"
	"github.com/golang/protobuf/proto"
	"github.com/golang/protobuf/protoc-gen-go/descriptor"
	jsoniter "github.com/json-iterator/go"
	"google.golang.org/protobuf/types/descriptorpb"
	"reflect"
)

type ModuleMetadataFactory interface {
	CreateFor(configType reflect.Type) *contract.ModuleMetadata
}

func NewModuleMetadataFactory() ModuleMetadataFactory {
	return &moduleMetadataFactory{}
}

type moduleMetadataFactory struct {
}

func (m *moduleMetadataFactory) CreateFor(configType reflect.Type) *contract.ModuleMetadata {
	stub := reflect.New(configType).Interface().(proto.Message)
	fileDescriptor, messageDescriptor := descriptor2.MessageDescriptorProto(stub)

	meta := &contract.ModuleMetadata{
		Modes: make([]*contract.CheckoutModeMetadata, 0),
		Config: &contract.ConfigDescriptor{
			MessageType: "." + fileDescriptor.GetPackage() + "." + messageDescriptor.GetName(),
			Fields:      make([]*contract.ConfigFieldDescriptor, 0, len(messageDescriptor.Field)),
		},
	}

	options := messageDescriptor.Options
	if name, err := proto.GetExtension(options, contract.E_CenturionModule); err != nil {
		panic("No module specified for " + configType.Name())
	} else {
		meta.Module = *name.(*contract.Module)
	}

	if version, err := proto.GetExtension(options, contract.E_CenturionVersion); err != nil {
		panic("No module version specified for " + configType.Name())
	} else {
		meta.Version = *version.(*string)
	}

	if displayName, err := proto.GetExtension(options, contract.E_CenturionModuleDisplayName); err != nil {
		panic("No module display name specified for " + configType.Name())
	} else {
		meta.DisplayName = *displayName.(*string)
	}

	modesOneOfIx := -1
	for ix, descr := range messageDescriptor.OneofDecl {
		if _, err := proto.GetExtension(descr.GetOptions(), contract.E_CenturionModuleModes); err == nil {
			modesOneOfIx = ix
			break
		}
	}

	for _, fieldDescr := range messageDescriptor.Field {
		if fieldDescr.OneofIndex != nil && fieldDescr.GetOneofIndex() == int32(modesOneOfIx) {
			if fieldDescr.GetType() != descriptorpb.FieldDescriptorProto_TYPE_MESSAGE {
				panic("Mode config must be custom message: " + fieldDescr.GetTypeName())
			}

			m.appendMode(fieldDescr.GetTypeName(), fileDescriptor, meta)
		} else {
			descr := createCfgFieldDescriptor(fieldDescr, fileDescriptor)
			meta.Config.Fields = append(meta.Config.Fields, descr)
		}
	}

	str, _ := jsoniter.MarshalIndent(meta, "", " ")
	println(string(str))

	return meta
}

func (m *moduleMetadataFactory) appendMode(typeName string, fileDescriptor *descriptor.FileDescriptorProto, meta *contract.ModuleMetadata) {
  modeDescr := getDescriptorByName(typeName, fileDescriptor)
  if name, err := proto.GetExtension(modeDescr.Options, contract.E_CenturionCheckoutMode); err != nil {
    panic("No mode name for message: " + typeName)
  } else {
    mode := &contract.CheckoutModeMetadata{
      Name:   *name.(*string),
      Config: createCfgDescriptor(typeName, fileDescriptor),
    }

    meta.Modes = append(meta.Modes, mode)
  }
}

func getDescriptorByName(typeName string, fileDescriptor *descriptor.FileDescriptorProto) *descriptorpb.DescriptorProto {
  var messageIx = -1
  for ix, descriptorProto := range fileDescriptor.GetMessageType() {
    messageFullName := "." + fileDescriptor.GetPackage() + "." + descriptorProto.GetName()
    if messageFullName == typeName {
      messageIx = ix
      break
    }
  }

  if messageIx == -1 {
    panic("Mode messages must be defined in same file as config. Couldn't find: " + typeName)
  }

  modeDescr := fileDescriptor.GetMessageType()[messageIx]
  return modeDescr
}

func createCfgDescriptor(typeName string, fileDescriptor *descriptor.FileDescriptorProto) *contract.ConfigDescriptor {
	modeDescr := getDescriptorByName(typeName, fileDescriptor)
	config := &contract.ConfigDescriptor{
		MessageType: typeName,
		Fields:      make([]*contract.ConfigFieldDescriptor, 0),
	}

	for _, fieldDescr := range modeDescr.Field {
		descr := createCfgFieldDescriptor(fieldDescr, fileDescriptor)
		config.Fields = append(config.Fields, descr)
	}

	return config
}

func createCfgFieldDescriptor(fieldDescr *descriptorpb.FieldDescriptorProto, fileDescriptor *descriptor.FileDescriptorProto) *contract.ConfigFieldDescriptor {
	cardinality := uint32(contract.Cardinality_CARDINALITY_NONE)
	if !fieldDescr.GetProto3Optional() {
		cardinality |= uint32(contract.Cardinality_REQUIRED)
	}

	if fieldDescr.GetLabel() == descriptorpb.FieldDescriptorProto_LABEL_REPEATED {
		cardinality |= uint32(contract.Cardinality_REPEATED)
	}

	fieldType := contract.Type(fieldDescr.GetType())
	var allowedValues []*contract.ReflectedAllowedValue = nil
	if fieldType == contract.Type_TYPE_ENUM && cardinality&uint32(contract.Cardinality_REPEATED) != 0 {
		ix := -1
		for eix, v := range fileDescriptor.GetEnumType() {
			enumName := "." + fileDescriptor.GetPackage() + "." + v.GetName()
			if fieldDescr.GetTypeName() == enumName {
				ix = eix
				break
			}
		}

		enm := fileDescriptor.EnumType[ix]
		if _, err := proto.GetExtension(enm.Options, contract.E_CenturionAllowedValuesDefinition); err == nil {
			if allowedValues, err = module_reflection.GetAllowedValuesFromEnum(enm); err != nil {
				panic(err)
			}
		}
	}

	descr := &contract.ConfigFieldDescriptor{
		Name:          fieldDescr.GetName(),
		Cardinality:   cardinality,
		Type:          fieldType,
		TypeName:      fieldDescr.GetTypeName(),
		AllowedValues: allowedValues,
	}
	return descr
}
