using System; 
// force a change for git push debugging .....

namespace StatePattern
{
    class MainApp
    {
        static void Main()
        {
            // Open a new account
            var account = new Account("Jim Johnson");

            // Apply financial transactions
            account.Deposit(500.0);
            account.Deposit(300.0);
            account.Deposit(550.0);
            account.PayInterest();
            account.Withdraw(2000.00);
            account.Withdraw(1100.00);
            account.Deposit(5000.00);

            // Wait for user
            Console.ReadKey();
        }
    }

    abstract class State
    {
        protected double interest;
        protected double lowerLimit;
        protected double upperLimit;

        // Gets or sets the account
        public Account Account { get; set; }

        // Gets or sets the balance
        public double Balance { get; set; }

        public abstract string Status { get; }

        public abstract void Deposit(double amount);
        public abstract void Withdraw(double amount);
        public abstract void PayInterest();
    }

    class RedState : State
    {
        double serviceFee;

        public override string Status => "Overdrawn";

        // Constructor
        public RedState(State state)
        {
            Balance = state.Balance;
            Account = state.Account;
            Initialize();
        }

        private void Initialize()
        {
            // Should come from a datasource
            interest = 0.0;
            lowerLimit = -100.0;
            upperLimit = 0.0;
            serviceFee = 10.00;
        }

        public override void Deposit(double amount)
        {
            Balance += amount;
            StateChangeCheck();
        }

        public override void Withdraw(double amount)
        {
            amount = amount - serviceFee;
            Console.WriteLine("No funds available for withdrawal!");
        }

        public override void PayInterest() { } // No interest is paid

        private void StateChangeCheck() { 
            if (Balance > upperLimit)
            {
                Account.State = new SilverState(this);
            }
        }
    }

    class SilverState : State
    {
        // Overloaded constructors

        public SilverState(State state) :
            this(state.Balance, state.Account)
        {
        }

        public SilverState(double balance, Account account)
        {
            Balance = balance;
            Account = account;
            Initialize();
        }

        public override string Status => "Normal";

        private void Initialize()
        {
            // Should come from a datasource
            interest = 0.0;
            lowerLimit = 0.0;
            upperLimit = 1000.0;
        }

        public override void Deposit(double amount)
        {
            Balance += amount;
            StateChangeCheck();
        }

        public override void Withdraw(double amount)
        {
            Balance -= amount;
            StateChangeCheck();
        }

        public override void PayInterest()
        {
            Balance += interest * Balance;
            StateChangeCheck();
        }

        private void StateChangeCheck()
        {
            if (Balance < lowerLimit)
            {
                Account.State = new RedState(this);
            }
            else if (Balance > upperLimit)
            {
                Account.State = new GoldState(this);
            }
        }
    }

    class GoldState : State
    {
        // Overloaded constructors
        public GoldState(State state)
            : this(state.Balance, state.Account)
        {
        }

        public GoldState(double balance, Account account)
        {
            Balance = balance;
            Account = account;
            Initialize();
        }

        public override string Status => "Interest Paying";

        private void Initialize()
        {
            // Should come from a database
            interest = 0.05;
            lowerLimit = 1000.0;
            upperLimit = 10000000.0;
        }

        public override void Deposit(double amount)
        {
            Balance += amount;
            StateChangeCheck();
        }

        public override void Withdraw(double amount)
        {
            Balance -= amount;
            StateChangeCheck();
        }

        public override void PayInterest()
        {
            Balance *= 1 + interest;  
            StateChangeCheck();
        }

        private void StateChangeCheck()
        {
            if (Balance < 0.0)
            {
                Account.State = new RedState(this);
            }
            else if (Balance < lowerLimit)
            {
                Account.State = new SilverState(this);
            }
        }
    }

    class Account
    {
        string owner;

        public Account(string owner)
        {
            // New accounts are 'Silver' by default
            this.owner = owner;
            this.State = new SilverState(0.0, this);
        }

        // Gets the balance
        public double Balance => State.Balance;

        public State State { get; set; }

        private enum TT { Deposit, Withdrawal };

        private void transact(double amount, TT tt = TT.Deposit)
        {
            if (tt == TT.Deposit)
            {
                State.Deposit(amount);
            }
            else if (tt == TT.Withdrawal)
            {
                State.Withdraw(amount);
            }
            Console.WriteLine($"{tt} {amount:C} --- ");
            Console.WriteLine($" Balance = {this.Balance:C}");
            Console.WriteLine($" Status  = {this.State.Status}");
            Console.WriteLine("");
        }

        public void Deposit(double amount) => transact(amount, TT.Deposit);

        public void Withdraw(double amount) => transact(amount, TT.Withdrawal);

        public void PayInterest()
        {
            State.PayInterest();
            Console.WriteLine("Interest Paid --- ");
            Console.WriteLine($" Balance = {this.Balance:C}");
            Console.WriteLine($" Status  = {this.State.Status}\n");
        }
    }
}
