using UnityEngine;
using System.Reflection;
using System.IO;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Config/Lyrics")]
    public class Lyrics : NodeBase
    {
        [Tooltip("The filename of the text file containing the lyrics")]
        [SerializeField]
        public string _fileName = "lyrics.txt";

        [Tooltip("Default lyrics will be used when no textfile has been found")]
        [TextAreaAttribute]
        [SerializeField]
        public string _defaultLyrics;

        [Inlet]
        public void all()
        {
            _textEvent.Invoke(_lyrics);
        }


        [Inlet]
        public string filename
        {
            set
            {
                _fileName = value;
                LoadLyrics();
            }
        }

        [Inlet]
        public float line
        {
            set
            {
                if (value >= 0 && value < _lines.Length)
                    _textEvent.Invoke(_lines[(int)value]);
            }
        }

        [Inlet]
        public float word
        {
            set
            {
                if (value >= 0 && value < _words.Length)
                    _textEvent.Invoke(_words[(int)value]);
            }
        }

        [SerializeField, Outlet]
        StringEvent _textEvent = new StringEvent();

        private string _lyrics;
        private string[] _lines;
        private string[] _words;

        private void LoadLyrics()
        {
            string file = FileMaster.GetFolder() + _fileName;
            if (File.Exists(file))
            {
                _lyrics = File.ReadAllText(file);
            }
            else
            {
                _lyrics = _defaultLyrics;
            }
            _lines = _lyrics.Split(new[] { '\r', '\n' });
            _words = _lyrics.Split(new[] { ' ', ',', '\r', '\n' });
        }

        public void Awake()
        {
            LoadLyrics();
        }
    }
}