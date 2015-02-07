using System;
using System.Collections.Generic;
using NumberType = System.Double;
using TestSimpleRNG;
using System.IO;
using Leap;

namespace KalmanCS
{
    /// <summary>
    /// </summary>
    public static class KalmanFilter
    {

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Distort our "actual" signal to generate the "measured" signal. This simulates
        /// measurement equipment not being perfect</summary>
        /// <param name="sent">The "actual" signal</param>
        /// <param name="measuredSigma">How inaccurate our measurements are, 0 for perfect 1 for
        /// terrible, higher for even worse.</param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public static NumberType[] Received(NumberType[] sent, NumberType measuredSigma)
        {
            NumberType[] measured = new NumberType[ sent.Length ];

            for( int curEntryIndex = 0; curEntryIndex < sent.Length; ++curEntryIndex )
                measured[curEntryIndex] = (NumberType)( sent[curEntryIndex] + SimpleRNG.GetNormal( 0D, (double)measuredSigma ) );

            return measured;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Search for "but where on earth did the pred_mean and pred_sigma functions come
        /// from" in the blog article for an explanation of this method.</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public static NumberType PredictedMean(NumberType sourceScale, NumberType previousMean)
        {
            return sourceScale * previousMean;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Search for "but where on earth did the pred_mean and pred_sigma functions come
        /// from" in the blog article for an explanation of this method.</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public static NumberType PredictedSigma(NumberType sourceScale, NumberType previousSigma, NumberType sourceSigma)
        {
            return (NumberType)Math.Sqrt( ( sourceScale * sourceScale ) * ( previousSigma * previousSigma ) + ( sourceSigma * sourceSigma ) );
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Explained at the beginning of the blog article</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public static NumberType UpdateMean(NumberType predictedMean, NumberType predictedSigma, NumberType measuredValue, NumberType measuredSigma)
        {
            NumberType numerator = ( predictedMean / ( predictedSigma * predictedSigma ) ) + ( measuredValue / ( measuredSigma * measuredSigma ) );

            NumberType denominator = ( (NumberType)1 / ( predictedSigma * predictedSigma ) ) + ( (NumberType)1 / ( measuredSigma * measuredSigma ) );

            return numerator / denominator;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Explained at the beginning of the blog article</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public static NumberType UpdateSigma(NumberType predictedSigma, NumberType measuredSigma)
        {
            double r = ( 1 / ( predictedSigma * predictedSigma ) ) + ( 1 / ( measuredSigma * measuredSigma ) );

            return (NumberType)( 1.0 / Math.Sqrt( r ) );
        }

        public static NumberType[] Filter(NumberType[] measuredValues, NumberType sourceScale, NumberType sourceSigma, NumberType measuredSigma)
        {
            NumberType lastMean = measuredValues[0];

            NumberType lastSigma = sourceSigma;

            NumberType[] filteredValues = new NumberType[50];


            for (int curEntryIndex = 0; curEntryIndex < 50; ++curEntryIndex)
            {
                NumberType estimatedMean = PredictedMean(sourceScale, lastMean);
                NumberType estimatedSigma = PredictedSigma(sourceScale, lastSigma, sourceSigma);

                filteredValues[curEntryIndex] = estimatedMean + (NumberType)SimpleRNG.GetNormal(0D, (double)estimatedSigma);

                lastMean = UpdateMean(estimatedMean, estimatedSigma, measuredValues[curEntryIndex], measuredSigma);
                lastSigma = UpdateSigma(estimatedSigma, measuredSigma);
            }

            return filteredValues;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Run a test of the Kalman filter</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public static Leap.Vector[] RunIt(NumberType sourceSigma, NumberType measuredSigma, Leap.Vector[] value)
        {
            NumberType sourceScale = (NumberType)Math.Sqrt(1 - (sourceSigma * sourceSigma));
            const int NumEntries = 50;

            KalmanResults results = new KalmanResults();
            Leap.Vector[] p = new Leap.Vector[50];
            for (int i = 0; i < 50; ++i)
                p[i] = new Leap.Vector();

            //calculate x
            for (int i = 0; i < 50; ++i)
                results.ActualValues[i] = value[i].x;

            results.MeasuredValues = results.ActualValues;

            // Apply the filter to that "measured" data
            results.FilteredValues = Filter(results.MeasuredValues, sourceScale, sourceSigma, measuredSigma);

            for (int i = 0; i < 50; ++i)
                p[i].x = (float)results.FilteredValues[i];

            //calculate y
            for (int i = 0; i < 50; ++i)
                results.ActualValues[i] = value[i].y;

            results.MeasuredValues = results.ActualValues;

            // Apply the filter to that "measured" data
            results.FilteredValues = Filter(results.MeasuredValues, sourceScale, sourceSigma, measuredSigma);
            for (int i = 0; i < 50; ++i)
                p[i].y = (float)results.FilteredValues[i];

            //calculate z
            for (int i = 0; i < 50; ++i)
                results.ActualValues[i] = value[i].z;

            results.MeasuredValues = results.ActualValues;

            // Apply the filter to that "measured" data
            results.FilteredValues = Filter(results.MeasuredValues, sourceScale, sourceSigma, measuredSigma);
            for (int i = 0; i < 50; ++i)
                p[i].z = (float)results.FilteredValues[i];

            return p;
        }
    }

    /// <summary>
    /// Represents the results of running a test with the Kalman filter
    /// </summary>
    public class KalmanResults
    {
        public NumberType[] ActualValues;
        public NumberType[] MeasuredValues;
        public NumberType[] FilteredValues;
        public KalmanResults()
        {
            ActualValues = new NumberType[50];
            MeasuredValues = new NumberType[50];
            FilteredValues = new NumberType[50];
        }
    }
}
