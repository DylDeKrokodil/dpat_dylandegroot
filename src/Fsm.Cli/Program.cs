using Fsm.Application;

var application = FsmApplication.CreateDefault(new ConsoleUserInterface());

return application.Run(args);
