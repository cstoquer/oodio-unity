using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class AudioRenderer : MonoBehaviour {
	public Slider slidderCut; 
	public Slider slidderRez; 

	private FreqTable freqTable;

	private int kCounter = 0;

	private OscRamp oscA;
	private RCFilter flt;


	private float env_time = 0.5f;
	private float env_inc = 0.5f * 0.01f;
	private float env_sta = 0.0f;
	public float Env_time {
		get {
			return env_time;
		}
		set {
			env_time = value;
			env_inc = value * 0.01f;
		}
	}

	private float clk_tmp;
	private float clk_pos = 0.0f;
	private float clk_inc = 0.0f;

	public  uint[] seq_stp;
	public  bool[] seq_trg;
	public  bool[] seq_kck;
	private uint   seq_pos;
	private float  seq_out;

	public  float Tempo {
		get {
			return clk_tmp;
		}
		set {
			clk_tmp = value;
			clk_inc = value / (15 * CONST.CONTROL_RATE);
		}
	}

	public AudioClip smpl_kick;
	private float[]  smpl_dat;
	private int      smpl_len;
	private int      smpl_pos = 0;


	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	// Use this for initialization
	void Start () {
		oscA = new OscRamp ();
		flt = new RCFilter ();

		flt.input = oscA.output; // TODO: connect method

		slidderCut.onValueChanged.AddListener ((float v) => {
			flt.cut = v;
		});

		slidderRez.onValueChanged.AddListener ((float v) => {
			flt.rez = v;
		});


		Tempo = 133f;

		freqTable = new FreqTable ();
		freqTable.Create ();

		// get samples data
		smpl_len = smpl_kick.samples;
		smpl_dat = new float[smpl_len];
		smpl_kick.GetData (smpl_dat, 0);
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void clk_tic () {
		clk_pos += clk_inc;
		if (clk_pos >= 1) {
			clk_pos -= 1;
			seq_tic();
		}
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void env_tic () {
		if (env_sta == 0f) return;
		env_sta -= env_inc;
		if (env_sta <= 0f) env_sta = 0f;
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void seq_tic () {
		if (++seq_pos >= seq_stp.Length) seq_pos = 0;
		seq_out = freqTable.GetNote(seq_stp[seq_pos]);
		oscA.Freq = seq_out;
		if (seq_trg[seq_pos]) env_sta = 1f;
		if (seq_kck[seq_pos]) smpl_pos = 0;
	}
	
	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	float smpl_tic() {
		if (smpl_pos >= smpl_len) return 0f;
		return smpl_dat[smpl_pos++];
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	void OnAudioFilterRead(float[] buffer, int channels) {
		for (var i = 0; i < buffer.Length; i += channels) {
			if (++kCounter == 64) {
				kCounter = 0;
				clk_tic();
				env_tic();
			}
			flt.Tic ();
			oscA.Tic();
			float val = env_sta * flt.output.signal + smpl_tic();
			//val *= 0.2f;
			buffer[i]     = val;
			buffer[i + 1] = val;
		}
	}
}
