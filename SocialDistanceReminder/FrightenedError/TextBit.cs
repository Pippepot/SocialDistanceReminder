namespace CoronaCaution
{
    struct TextBit
    {
        public string text;     // Text to display
        public double delay;    // How many seconds before one character shows up
        public double wait;     // How many seconds to wait for a new text to show up, after the text is fully rendered

        public TextBit(string _text, double _delay, double _wait)
        {
            text = _text;
            delay = _delay;
            wait = _wait;
        }
    }
}
