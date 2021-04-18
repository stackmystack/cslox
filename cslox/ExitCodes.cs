namespace cslox
{
    public class SysExitCode
    {
        public static readonly int OK = 0;           /* successful termination */
        public static readonly int BASE = 64;        /* base value for error messages */
        public static readonly int USAGE = 64;       /* command line usage error */
        public static readonly int DATAERR = 65;     /* data format error */
        public static readonly int NOINPUT = 66;     /* cannot open input */
        public static readonly int NOUSER = 67;      /* addressee unknown */
        public static readonly int NOHOST = 68;      /* host name unknown */
        public static readonly int UNAVAILABLE = 69; /* service unavailable */
        public static readonly int SOFTWARE = 70;    /* internal software error */
        public static readonly int OSERR = 71;       /* system error (e.g., can't fork) */
        public static readonly int OSFILE = 72;      /* critical OS file missing */
        public static readonly int CANTCREAT = 73;   /* can't create (user) output file */
        public static readonly int IOERR = 74;       /* input/output error */
        public static readonly int TEMPFAIL = 75;    /* temp failure; user is invited to retry */
        public static readonly int PROTOCOL = 76;    /* remote error in protocol */
        public static readonly int NOPERM = 77;      /* permission denied */
        public static readonly int CONFIG = 78;      /* configuration error */
        public static readonly int MAX = 78;	     /* maximum listed value */
    }
}