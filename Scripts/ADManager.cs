#if USING_GOOGLE_AD_MOB
using GoogleMobileAds.Api;
using System;
using UnityEngine;

namespace Parang
{
    public class ADManager : Singleton<ADManager>
    {
        public string AndroidAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        public string IOSAdUnitId = "ca-app-pub-3940256099942544/4411468910";

        private string _adUnitId = "unused";
        private InterstitialAd _interstitialAd;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();

#if UNITY_ANDROID
            _adUnitId = AndroidAdUnitId;
#elif UNITY_IPHONE
            _adUnitId = IOSAdUnitId;
#else
            _adUnitId = "unused";
#endif
            MobileAds.Initialize((InitializationStatus status) =>
            {
                LoadInterstitialAd();
            });
        }

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        public void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");

            // send the request to load the ad.
            InterstitialAd.Load(_adUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitialAd = ad;
                });
        }

        /// <summary>
        /// Shows the interstitial ad.
        /// </summary>
        public bool ShowAd(Action onAdPaid)
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                RegisterEventHandlers(_interstitialAd, onAdPaid);
                _interstitialAd.Show();
                return true;
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
                return false;
            }
        }

        private void RegisterEventHandlers(InterstitialAd ad, Action onAdPaid)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
                onAdPaid?.Invoke();
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
                SoundManager.Instance.SetMasterVolume(0f);
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadInterstitialAd();
                SoundManager.Instance.SetMasterVolume(1f);
#if UNITY_EDITOR
                onAdPaid?.Invoke();
#endif
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                LoadInterstitialAd();
                SoundManager.Instance.SetMasterVolume(1f);
            };
        }
    }
}
#endif