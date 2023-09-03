using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static BasketApi.Global;
using System.Data.Entity;
using WebApplication1.ViewModels;
using System.Configuration;

namespace BasketApi
{
    public static class ExtensionMethods
    {
        public static void CreateSignature(this string StringToSha, PayFortConfiguration configuration, CaptureBindingModel captureModel, VoidAuthorizationBindingModel VoidModel, SDKTokenModel tokenModel)
        {
            if (captureModel != null)
            {
                var SignatureToSha = configuration.sha_request_phrase + "access_code=" + captureModel.access_code + "amount=" + captureModel.amount + "command=" + captureModel.command + "currency=" + captureModel.currency + "fort_id=" + captureModel.fort_id + "language=" + captureModel.language + "merchant_identifier=" + captureModel.merchant_identifier + "merchant_reference=" + captureModel.merchant_reference + configuration.sha_request_phrase;
                captureModel.signature = Utility.sha256_hash(SignatureToSha);
            }
            else if (VoidModel != null)
            {
                var SignatureToShaVoid = configuration.sha_request_phrase + "access_code=" + VoidModel.access_code + "command=" + VoidModel.command + "fort_id=" + VoidModel.fort_id + "language=" + VoidModel.language + "merchant_identifier=" + VoidModel.merchant_identifier + "merchant_reference=" + VoidModel.merchant_reference + configuration.sha_request_phrase;
                VoidModel.signature = Utility.sha256_hash(SignatureToShaVoid);
            }
            else
            {
                var SignatureToShaSDK = configuration.sha_request_phrase + "access_code=" + tokenModel.access_code + "device_id=" + tokenModel.device_id + "language=" + tokenModel.language + "merchant_identifier=" + tokenModel.merchant_identifier + "service_command=" + tokenModel.service_command + configuration.sha_request_phrase;
                tokenModel.signature = Utility.sha256_hash(SignatureToShaSDK);
            }
        }

        //public static void GetPayFortConfiguration(this PayFortConfiguration model, CaptureBindingModel captureModel)
        //{
        //    try
        //    {
        //        BasketSettings.LoadSettings();

        //        model.access_code = BasketSettings.Settings.Payfort_AccessCode;
        //        model.merchant_identifier = BasketSettings.Settings.Payfort_MerchantId;
        //        model.sha_type = BasketSettings.Settings.SHA_TYPE;
        //        model.sha_request_phrase = BasketSettings.Settings.Payfort_RequestPhrase;
        //        model.sha_response_phrase = BasketSettings.Settings.Payfort_ResponsePhrase;
        //        model.language = "en";
        //        model.currency = "AED";
                
        //        captureModel.access_code = model.access_code;
        //        captureModel.merchant_identifier = model.merchant_identifier;

        //        captureModel.currency = model.currency;
        //        captureModel.language = model.language;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //public static void GetPayFortConfigurationForMobileSDK(this PayFortConfiguration model, SDKTokenModel sdkModel)
        //{
        //    try
        //    {
        //        BasketSettings.LoadSettings();

        //        model.access_code = BasketSettings.Settings.Payfort_AccessCode;
        //        model.merchant_identifier = BasketSettings.Settings.Payfort_MerchantId;
        //        model.sha_type = BasketSettings.Settings.SHA_TYPE;
        //        model.sha_request_phrase = BasketSettings.Settings.Payfort_RequestPhrase;
        //        model.sha_response_phrase = BasketSettings.Settings.Payfort_ResponsePhrase;
        //        model.language = "en";
        //        model.currency = "AED";

        //        sdkModel.access_code = model.access_code;
        //        sdkModel.merchant_identifier = model.merchant_identifier;
        //        sdkModel.language = model.language;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

    

     
        public static async Task GetRandomActivationCode(this UserSubscriptions userSubscription)
        {
            var crypto = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var bytes = new byte[5];
            crypto.GetBytes(bytes);
            userSubscription.ActivationCode = BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}