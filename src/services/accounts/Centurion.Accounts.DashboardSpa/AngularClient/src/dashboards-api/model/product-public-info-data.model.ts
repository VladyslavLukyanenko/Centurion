/**
 * Dashboards API
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { ProductFeatureData } from './product-feature-data.model';


export interface ProductPublicInfoData { 
    name?: string | null;
    description?: string | null;
    logoSrc?: string | null;
    imageSrc?: string | null;
    version?: string | null;
    features?: Array<ProductFeatureData> | null;
}
