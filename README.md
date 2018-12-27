# cpcontrib.logging
This library strives to achieve a subset of NLog style logging within CrownPeak

```
paket install cpcontrib.logging
cppm install cpcontrib.logging
```

cpcontrib.logging is designed to be similar to NLog logging framework (see NLog).

We've also included two non-NLog-compliant loggers for sending Email and using CrownPeak's Util.Log sink, so CMS developers can start utilizing some better logging for the time being.  Both loggers support Is[Level]Enabled calls, and calls to specific levels like `Debug(message)` or `Info(message)`.  They also support the messageGeneratorFunc overloads similar to NLog level logging functions.

### EmailLogger

EmailLogger sends a email with all the log messages after calling Flush or Dispose.  Best to use a using statement to ensure Dispose is called.

```c#
<%
  Log = new EmailLogger(subject:"Logging sample", receipients:"somebody@cp.com;others@another.com");
  //Log.IsDebugEnabled = true; //use this to increase logging level to Debug
  
  try
  {
    //code here
  }
  finally
  {
    Log.Dispose();
  }
%>
```

### UtilLogLogger

The UtilLogLogger utilizes the Util.Log method for recording log entries to history.  Two options, to log into a specific Asset's history or the System's history.  

```c#
//UtilLogLogger can utilize current asset, or log to System Log (no asset) by using other constructor
Log UtilLogLogger = new UtilLogLogger("Component Name",asset);  //Log entries go into asset's history

Log UtilLogLogger = new UtilLogLogger("Component Name"); //Log entries go into System history
```

When using the Buffered version (by default), setting Buffered=true, all the log messages are stored up until Flush or Dispose are called.  Setting Buffered=false will cause every Log message to be immediately run through the Util.Log underlying method.  This will produce a chatty history, and might be what is desired.
```c#
Log UtilLogLogger = new UtilLogLogger("Component Name",asset) { Buffered = false };

Log.Debug("Test");
Log.Info("Info Test"); //will see this go immediately into History
```
