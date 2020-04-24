

public static class UIDele
{
	public delegate void Dele();
	public delegate void Dele<Pram0>(Pram0 t);
	public delegate void Dele<Pram0,Pram1>(Pram0 t,Pram1 v);
	public delegate void Dele<Pram0,Pram1,Pram2>(Pram0 t,Pram1 v,Pram2 w);

	public delegate bool BoolDele<Pram0>(Pram0 t);
}