public class RCFilter {
	public  float cut { get; set; }
	public  float rez { get; set; }
	private float state;

	public AudioSignal input;
	public AudioSignal output;
	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	public RCFilter() {
		cut    = 0.5f;
		rez    = 0.7f;
		state  = 0f;
		input  = new AudioSignal(); // TODO should we have a global signal for not connected connector ?
		output = new AudioSignal();
	}
	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	public void Tick() {
		float t = 1 - rez * cut;
		state = t * state - cut * output.signal + cut * input.signal;
		output.signal = t * output.signal + cut * state;
	}
}

	