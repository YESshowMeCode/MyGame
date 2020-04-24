using System;

public class ProcessData : ICloneable
{
	public object Clone()
	{
		return this.MemberwiseClone();
	}
}

public abstract class CodeStrategy
{
	public void ProcessBefore(ProcessData data)
	{
		processBefore(data);
	}

	public void Process(ProcessData data)
	{
		process(data);
	}

	public void ProcessAfter(ProcessData data)
	{
		processAfter(data);
	}


	protected abstract void processBefore(ProcessData data);
	protected abstract void process(ProcessData data);
	protected abstract void processAfter(ProcessData data);
}


public abstract class CodeStrategy<T> : CodeStrategy where T : ProcessData {

	protected sealed override void processBefore(ProcessData data)
	{
		processBeforeIn(data as T);
	}

	protected sealed override void process(ProcessData data)
	{
		processIn(data as T);
	}

	protected sealed override void processAfter(ProcessData data)
	{
		processAfterIn(data as T);
	}



	protected abstract  void processBeforeIn(T data);
	protected abstract  void processIn(T data);
	protected abstract  void processAfterIn(T data);


	public abstract string GetInitClassName(string objName, string nodeTypeStr);
}
