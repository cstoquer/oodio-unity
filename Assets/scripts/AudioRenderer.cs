using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class AudioRenderer : MonoBehaviour {
	public AudioClip kickSample;

	public Slider slidderCut; 
	public Slider slidderRez; 

	private FreqTable freqTable;

	private int kCounter = 0;

	private OscRamp  oscA;
	private RCFilter flt;
	private Sampler  kickDrum;
	private Envelope vca;

	private float clk_tmp;
	private float clk_pos = 0.0f;
	private float clk_inc = 0.0f;

	[Header("Notes")]
	public  uint[] seq_stp;
	[Header("Retriger")]
	public  bool[] seq_trg;
	[Header("Kick Drum")]
	public  bool[] seq_kck;


	private uint   seq_pos;
	private float  seq_out;

	public  float Tempo {
		get {
			return clk_tmp;
		}
		set {
			clk_tmp = value;
			clk_inc = value / (15 * CONSTANTS.CONTROL_RATE);
		}
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	// Use this for initialization
	void Start () {
		oscA = new OscRamp();
		flt = new RCFilter();
		kickDrum = new Sampler(kickSample);
		vca = new Envelope();

		flt.input = oscA.output; // TODO: connect method

		slidderCut.onValueChanged.AddListener((float v) => {
			flt.cut = v;
		});

		slidderRez.onValueChanged.AddListener((float v) => {
			flt.rez = v;
		});


		Tempo = 133f;

		freqTable = new FreqTable ();
		freqTable.Create ();
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void clk_tick () {
		clk_pos += clk_inc;
		if (clk_pos >= 1) {
			clk_pos -= 1;
			seq_tick();
		}
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void seq_tick () {
		if (++seq_pos >= seq_stp.Length) seq_pos = 0;
		seq_out = freqTable.GetNote(seq_stp[seq_pos]);
		oscA.Freq = seq_out;
		if (seq_trg[seq_pos]) vca.Retrig();
		if (seq_kck [seq_pos]) kickDrum.Retrig();
	}
	
	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void OnAudioFilterRead(float[] buffer, int channels) {
		for (var i = 0; i < buffer.Length; i += channels) {
			if (++kCounter == 64) {
				kCounter = 0;
				clk_tick();
				vca.Tick();
			}
			flt.Tick ();
			oscA.Tick();
			kickDrum.Tick ();
			float val = vca.output.signal * flt.output.signal + kickDrum.output.signal;
			//val *= 0.2f;
			buffer[i]     = val;
			buffer[i + 1] = val;
		}
	}
}
