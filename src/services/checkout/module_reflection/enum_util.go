package module_reflection

import (
  "errors"
  "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/golang/protobuf/proto"
  "google.golang.org/protobuf/types/descriptorpb"
)

func GetAllowedValuesFromEnum(enm *descriptorpb.EnumDescriptorProto) ([]*contracts.ReflectedAllowedValue, error) {
  if _, err := proto.GetExtension(enm.Options, contracts.E_CenturionAllowedValuesDefinition); err == nil {
    allowedValues := make([]*contracts.ReflectedAllowedValue, 0, len(enm.Value))
    for _, ev := range enm.GetValue() {
      if boxedAllowedVal, err := proto.GetExtension(ev.Options, contracts.E_CenturionAllowedValue); err != nil {
        return nil, errors.New("No allowed value specified for " + enm.GetName() + "." + ev.GetName())
      } else {
        allowedValues = append(allowedValues, &contracts.ReflectedAllowedValue{
          Value:     boxedAllowedVal.(*contracts.AllowedValue),
          Index: ev.GetNumber(),
        })
      }
    }

    return allowedValues, nil
  }

  return nil, errors.New("not enabled values definition")
}

func GetAllowedValuesFromEnumMap(enm *descriptorpb.EnumDescriptorProto) (map[int32]*contracts.ReflectedAllowedValue, error) {
  if _, err := proto.GetExtension(enm.Options, contracts.E_CenturionAllowedValuesDefinition); err == nil {
    allowedValues := map[int32]*contracts.ReflectedAllowedValue{}
    for _, ev := range enm.GetValue() {
      if boxedAllowedVal, err := proto.GetExtension(ev.Options, contracts.E_CenturionAllowedValue); err != nil {
        return nil, errors.New("No allowed value specified for " + enm.GetName() + "." + ev.GetName())
      } else {
        allowedValue := boxedAllowedVal.(*contracts.AllowedValue)
        allowedValues[ev.GetNumber()] = &contracts.ReflectedAllowedValue{
          Value:     allowedValue,
          Index: ev.GetNumber(),
        }
      }
    }

    return allowedValues, nil
  }

  return nil, errors.New("not enabled values definition")
}
