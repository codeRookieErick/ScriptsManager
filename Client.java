//test 

public class Client{
	public static void main(String[] args)
	{
		Client c = new Client();
		int t = c.MakeATest();
		System.out.println(String.valueOf(t));
	}
	
	private native int MakeATest();
	
	static{
		System.loadLibrary("Cliente");
	}
}
