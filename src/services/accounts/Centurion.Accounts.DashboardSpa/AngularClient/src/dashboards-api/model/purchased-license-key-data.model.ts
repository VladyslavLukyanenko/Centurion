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
import { UserRef } from './user-ref.model';


export interface PurchasedLicenseKeyData { 
    id?: number;
    user?: UserRef;
    value?: string | null;
    expiry?: string;
    releaseTitle?: string | null;
    planDesc?: string | null;
    hasActiveSession?: boolean;
    isUnbindable?: boolean;
    lastAuthRequest?: string;
    isSubscriptionCancelled?: boolean;
    isExpired?: boolean;
}

