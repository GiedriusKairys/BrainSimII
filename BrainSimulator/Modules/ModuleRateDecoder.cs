﻿//
// Copyright (c) [Name]. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator.Modules
{
    public class ModuleRateDecoder : ModuleBase
    {
        //any public variable you create here will automatically be saved and restored  with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;


        //set size parameters as needed in the constructor
        //set max to be -1 if unlimited

        float theLeakRate = 0.13f;

        public ModuleRateDecoder()
        {
            minHeight = 2;
            maxHeight = 15;
            minWidth = 3;
            maxWidth = 3;
        }


        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here

            //if you want the dlg to update, use the following code whenever any parameter changes
            // call UpdateDialog
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
            Init();
            SetUpNeurons(mv.Height - 1);
        }

        private void SetUpNeurons(int levelCount)
        {
            Neuron nIn = mv.GetNeuronAt(1,0 );
            nIn.Label = "In";
            Neuron nRd = mv.GetNeuronAt(0, 0);
            nRd.Label = "Rd";
            Neuron nClr = mv.GetNeuronAt(2, 0);
            nClr.Label = "Clr";

            nRd.AddSynapse(nClr.id, 1);
            Neuron nLast = mv.GetNeuronAt(0, mv.Height - 1);


            for (int i = 0; i < levelCount; i++)
            {
                Neuron ni = mv.GetNeuronAt(0, i + 1);
                ni.Clear();
                Neuron nm = mv.GetNeuronAt(1, i + 1);
                ni.Clear();
                Neuron no = mv.GetNeuronAt(2, i + 1);
                no.Clear();
            }

            for (int i = 0; i < levelCount; i++)
            {
                Neuron ni = mv.GetNeuronAt(0, i + 1);
                ni.Model = Neuron.modelType.LIF;
                ni.LeakRate = theLeakRate;
                Neuron nm = mv.GetNeuronAt(1, i + 1);
                Neuron no = mv.GetNeuronAt(2, i + 1);
                no.Label = "O" + i;

                ni.AddSynapse(nm.id, 1);
                nm.AddSynapse(no.id, 0.9f);
                no.AddSynapse(nm.id, 1);

                nClr.AddSynapse(no.id, -1f);
                nRd.AddSynapse(no.id, 0.9f);
                nLast.AddSynapse(no.id, -1f);

                float weight = GetWeight(4 + i);
                weight += .001f; //differentiates between < and = 
                nIn.AddSynapse(ni.id, weight);

                for (int j = i + 1; j < levelCount; j++)
                {
                    ni.AddSynapse(mv.GetNeuronAt(1, j + 1).id, -1f);
                }
                for (int j = 0; j < levelCount; j++)
                {
                    if (j != i)
                        ni.AddSynapse(mv.GetNeuronAt(0, j + 1).id, -1f);
                }
            }
        }

        float GetWeight(int count)
        {
            float decayFactor = (float)Math.Pow((1 - theLeakRate), count);
            float w = 1 / (1 + decayFactor);
            return w;
        }

        
        //called whenever the size of the module rectangle changes, delete if not needed
        //for example, you may choose to reinitialize whenever size changes
        //delete if not needed
        public override void SizeChanged()
        {
            if (mv == null) return; //things aren't initialized yet
            SetUpNeurons(mv.Height - 1);
        }
    }
}
