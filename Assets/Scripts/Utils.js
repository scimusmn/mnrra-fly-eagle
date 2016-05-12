#pragma strict

public static class Utils {

	// Map a value from one range to another
	public function Map ( val:float, a1:float, a2:float, b1:float, b2:float ) {

		return b1 + (val - a1) * (b2 - b1) / (a2 - a1);

	}

}