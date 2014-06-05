using System;
using System.Threading;

// Gebaseerd op werk van Mark Fluitsma

namespace HWW3
{
    // Parameters die je kunt veranderen om queue te testen

    // Als alles lijkt te werken, moet je NBOXES maar eens op 1000 zetten en TICK op 10
    
    class CONST
    {
        public const int NPRODUCERS = 6;
        public const int NCONSUMERS = 6;
        public const int TICK = 10; // de basis-tijdeenheid in miliseconden
        public const int NBOXES = 1000; // aantal dozen dat een producent maakt en een consument gebruikt
        public const int BUFLEN = 5;  // de lengte van de queue
    }

    // Klasses Consumer en Producer; volgens mij in orde
    abstract class Worker
    {
        protected Queue q;
        protected String name;

        public Worker()
        {
            Thread workerThread = new Thread(work);
            workerThread.Start();
        }
        protected int random(int n)
        {
            return new Random().Next() % n;
        }
        public abstract void handleBox(int i);
        
        private void work()
        {
            for (int i = 1; i <= CONST.NBOXES; i++)
            {
                handleBox(i);
                Thread.Sleep(random(CONST.TICK));
            }
        }
    }

    class Producer : Worker
    {
        public Producer(String name, Queue q)
        {
            this.name = name;
            this.q = q;
        }

        public override void handleBox(int i)
        {
            Box box = new Box(name, i);
            q.Put(name, box);
        }

    }

    class Consumer : Worker
    {
        public Consumer(String name, Queue q)
        {
            this.name = name;
            this.q = q;
        }
        public override void handleBox(int i)
        {
            Box box = q.Get(name);
        }
    }

    // Klasse Box: eenvoudig doosje. Niks meer aan doen.
    // Bevat alleen de naam van de producent en een volgnummer
    class Box
    {
        private string name;
        private int number;

        public Box(string name, int number)
        {
            this.name = name;
            this.number = number;
        }

        public string Id
        {
            get
            {
                return name + " - " + number;
            }
        }
    }

    // De ronde wachtrij
    // Die moet je dus thread-safe maken en voorzien van semaforen...
	//class Queue
	//{
	//	private Semaphore put = new Semaphore(CONST.BUFLEN, CONST.BUFLEN); //Er kan maar CONST.BUFLEN in de Queue;
	//	private Semaphore get = new Semaphore(0, CONST.BUFLEN); //Er kan niet meer uit dan in
	//	private Semaphore busy = new Semaphore(1, 1);


	//	private Box[] buffer = new Box[CONST.BUFLEN];

	//	// Twee indexen en de lengte bijhouden.
	//	// Redundant, maar lekker makkelijk!
	//	private int getpos, putpos;
	//	private int count;

	//	public Queue()
	//	{
	//		getpos = 0;
	//		putpos = 0;
	//		count = 0;

	//	}

	//	public Box Get(string consumername)
	//	{
	//		get.WaitOne(); // Staat er iets in de Queue?

	//		busy.WaitOne(); // lock {}
	//		Box box = buffer[getpos];
	//		getpos = (getpos + 1) % CONST.BUFLEN;
	//		count--;
	//		Console.WriteLine(consumername + ": gets " + box.Id);

	//		put.Release(); //Er is iets uit de Queue gehaald dus er kan weer iets in
	//		busy.Release(); // Klaar
	//		return box;
	//	}

	//	public void Put(string producername, Box box)
	//	{
	//		put.WaitOne(); // Is er nog plek in de Queue?

	//		busy.WaitOne(); // lock {}
	//		Console.WriteLine(producername + ": puts " + box.Id);
	//		buffer[putpos] = box;
	//		putpos = (putpos + 1) % CONST.BUFLEN;
	//		count++;
			
	//		get.Release(); // Er is een item toegevoegd, er kan er dus 1 weer uit worden gehaald
	//		busy.Release(); // Klaar
	//	}
	//}

	class Queue
	{
		private Semaphore put = new Semaphore(CONST.BUFLEN, CONST.BUFLEN); //Er kan maar CONST.BUFLEN in de Queue;
		private Semaphore get = new Semaphore(0, CONST.BUFLEN); //Er kan niet meer uit dan in

		private Box[] buffer = new Box[CONST.BUFLEN];

		// Twee indexen en de lengte bijhouden.
		// Redundant, maar lekker makkelijk!
		private int getpos, putpos;
		private int count;

		public Queue()
		{
			getpos = 0;
			putpos = 0;
			count = 0;

		}

		public Box Get(string consumername)
		{
			get.WaitOne(); // Staat er iets in de Queue?
			lock (buffer) 
			{
				Box box = buffer[getpos];
				getpos = (getpos + 1) % CONST.BUFLEN;
				count--;
				Console.WriteLine(consumername + ": gets " + box.Id);

				put.Release(); //Er is iets uit de Queue gehaald dus er kan weer iets in
				return box;
			}
		}

		public void Put(string producername, Box box)
		{
			put.WaitOne(); // Is er nog plek in de Queue?
			lock (buffer)
			{
				Console.WriteLine(producername + ": puts " + box.Id);
				buffer[putpos] = box;
				putpos = (putpos + 1) % CONST.BUFLEN;
				count++;

				get.Release(); // Er is een item toegevoegd, er kan er dus 1 weer uit worden gehaald
			}
			
		}
	}

    // Test it...
    class Program
    {
        static void Main(string[] args)
        {
            Queue q = new Queue();

            for (int i = 1; i <= CONST.NPRODUCERS; i++)
                new Producer("P" + i, q);

            for (int i = 1; i <= CONST.NCONSUMERS; i++)
                new Consumer("C" + i, q);

            Console.ReadLine();
        }
    }
}
