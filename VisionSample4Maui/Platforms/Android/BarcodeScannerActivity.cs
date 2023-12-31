﻿using Android.App;
using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using Android.OS;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionSample4Maui.Platforms.Android
{
    [Activity(Label = "BarcodeScannerActivity")]
    public class BarcodeScannerActivity : Activity
    {
        const string TAG = "FaceTracker";

        CameraSource mCameraSource;
        CameraSourcePreview mPreview;
        GraphicOverlay mGraphicOverlay;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.FaceTracker);

            mPreview = FindViewById<CameraSourcePreview>(Resource.Id.preview);
            mGraphicOverlay = FindViewById<GraphicOverlay>(Resource.Id.faceOverlay);

            var detector = new BarcodeDetector.Builder(Application.Context)
                .Build();
            detector.SetProcessor(
                new MultiProcessor.Builder(new GraphicBarcodeTrackerFactory(mGraphicOverlay)).Build());

            if (!detector.IsOperational)
            {
                // Note: The first time that an app using barcode API is installed on a device, GMS will
                // download a native library to the device in order to do detection.  Usually this
                // completes before the app is run for the first time.  But if that download has not yet
                // completed, then the above call will not detect any barcodes.
                //
                // IsOperational can be used to check if the required native library is currently
                // available.  The detector will automatically become operational once the library
                // download completes on device.
                //Android.Util.Log.Warn(TAG, "Barcode detector dependencies are not yet available.");
            }

            mCameraSource = new CameraSource.Builder(Application.Context, detector)
                .SetRequestedPreviewSize(640, 480)
                .SetFacing(CameraFacing.Back)
                .SetRequestedFps(15.0f)
                .Build();
        }

        protected override void OnResume()
        {
            base.OnResume();

            StartCameraSource();
        }

        protected override void OnPause()
        {
            base.OnPause();

            mPreview.Stop();
        }

        protected override void OnDestroy()
        {
            mCameraSource.Release();

            base.OnDestroy();
        }


        //==============================================================================================
        // Camera Source Preview
        //==============================================================================================

        /**
     * Starts or restarts the camera source, if it exists.  If the camera source doesn't exist yet
     * (e.g., because onResume was called before the camera source was created), this will be called
     * again when the camera source is created.
     */
        void StartCameraSource()
        {
            try
            {
                mPreview.Start(mCameraSource, mGraphicOverlay);
            }
            catch (Exception e)
            {
                //Android.Util.Log.Error(TAG, "Unable to start camera source.", e);
                mCameraSource.Release();
                mCameraSource = null;
            }
        }

        //==============================================================================================
        // Graphic Face Tracker
        //==============================================================================================

        /**
     * Factory for creating a face tracker to be associated with a new face.  The multiprocessor
     * uses this factory to create face trackers as needed -- one for each individual.
     */
        class GraphicBarcodeTrackerFactory : Java.Lang.Object, MultiProcessor.IFactory
        {
            public GraphicBarcodeTrackerFactory(GraphicOverlay overlay) : base()
            {
                Overlay = overlay;
            }

            public GraphicOverlay Overlay { get; private set; }

            public Tracker Create(Java.Lang.Object item)
            {
                return new GraphicBarcodeTracker(Overlay);
            }
        }

        /**
     * Face tracker for each detected individual. This maintains a face graphic within the app's
     * associated face overlay.
     */
        class GraphicBarcodeTracker : Tracker
        {
            GraphicOverlay mOverlay;
            BarcodeGraphic mBarcodeGraphic;

            public GraphicBarcodeTracker(GraphicOverlay overlay)
            {
                mOverlay = overlay;
                mBarcodeGraphic = new BarcodeGraphic(overlay);
            }

            /**
            * Start tracking the detected face instance within the face overlay.
            */
            public override void OnNewItem(int idValue, Java.Lang.Object item)
            {
                mBarcodeGraphic.Id = idValue;
            }

            /**
            * Update the position/characteristics of the face within the overlay.
            */
            public override void OnUpdate(Detector.Detections detections, Java.Lang.Object item)
            {
                mOverlay.Add(mBarcodeGraphic);
                mBarcodeGraphic.UpdateBarcode(item.JavaCast<Barcode>());
            }

            /**
            * Hide the graphic when the corresponding face was not detected.  This can happen for
            * intermediate frames temporarily (e.g., if the face was momentarily blocked from
            * view).
            */
            public override void OnMissing(Detector.Detections detections)
            {
                mOverlay.Remove(mBarcodeGraphic);
            }

            /**
            * Called when the face is assumed to be gone for good. Remove the graphic annotation from
            * the overlay.
            */
            public override void OnDone()
            {
                mOverlay.Remove(mBarcodeGraphic);
            }
        }
    }
}
