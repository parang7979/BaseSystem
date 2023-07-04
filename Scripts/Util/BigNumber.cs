using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parang.Util
{
    public struct BigNumber
    {
        static private readonly uint _unit = 1000;
        static private readonly uint _unitSq = _unit * _unit;
        static private readonly uint _unitDigit = 3;

        static public readonly BigNumber Zero = new BigNumber(0);
        static public readonly BigNumber IntMax = new BigNumber(int.MaxValue);
        static public readonly BigNumber LongMax = new BigNumber(long.MaxValue);

        public long Value
        {
            get => (long)_value;
            set
            {
                _value = (ulong)value;
                Calc();
            }
        }

        public long Digit { get => (long)_digit; set => _digit = (ulong)value; }
        public ulong RealDigit => (ulong)Math.Log10(_value) + (ulong)Digit * _unitDigit;

        private ulong _value;
        private ulong _digit;

        public BigNumber(long value, long digit)
        {
            _value = (ulong)(value * Math.Pow(10, digit % _unitDigit));
            _digit = (ulong)(digit / _unitDigit);
            Calc();
        }

        public BigNumber(int value)
        {
            _value = (ulong)value;
            _digit = 0;
            Calc();
        }

        public BigNumber(long value)
        {
            _value = (ulong)value;
            _digit = 0;
            Calc();
        }

        static public int ToInt(BigNumber v)
        {
            if (v < IntMax)
                return (int)(v._value * (ulong)Math.Pow(_unit, v._digit));
            return int.MaxValue;
        }

        static public long ToLong(BigNumber v)
        {
            if (v < LongMax)
                return (long)(v._value * (ulong)Math.Pow(_unit, v._digit));
            return long.MaxValue;
        }

        static public void DigitBlance(ref BigNumber lhs, ref BigNumber rhs)
        {
            if (rhs._digit < lhs._digit)
            {
                var diff = lhs._digit - rhs._digit;
                if (diff == 2)
                {
                    lhs._value *= _unitSq;
                    lhs._digit -= 2;
                }
                else if (diff == 1)
                {
                    lhs._value *= _unit;
                    lhs._digit -= 1;
                }
            }
            else if (lhs._digit < rhs._digit)
            {
                var diff = rhs._digit - lhs._digit;
                if (diff == 2)
                {
                    rhs._value *= _unitSq;
                    rhs._digit -= 2;
                }
                else if (diff == 1)
                {
                    rhs._value *= _unit;
                    rhs._digit -= 1;
                }
            }
        }

        static public BigNumber operator +(BigNumber lhs, BigNumber rhs)
        {
            DigitBlance(ref lhs, ref rhs);
            if (lhs._digit != rhs._digit)
                return lhs._digit > rhs._digit ? lhs : rhs;
            lhs._value += rhs._value;
            lhs.Calc();
            return lhs;
        }

        static public BigNumber operator -(BigNumber lhs, BigNumber rhs)
        {
            DigitBlance(ref lhs, ref rhs);
            if (lhs._digit != rhs._digit)
                return lhs._digit > rhs._digit ? lhs : rhs;
            if (lhs._value < rhs._value) return Zero;
            lhs._value -= rhs._value;
            lhs.Calc();
            return lhs;
        }

        static public BigNumber operator *(BigNumber lhs, int rhs)
        {
            lhs._value *= (ulong)rhs;
            lhs.Calc();
            return lhs;
        }

        static public BigNumber operator *(BigNumber lhs, ulong rhs)
        {
            lhs._value *= rhs;
            lhs.Calc();
            return lhs;
        }

        static public BigNumber operator *(BigNumber lhs, float rhs)
        {
            var k = lhs._value * rhs;
            if (k < 1f)
            {
                lhs._value = (ulong)(k * _unit);
                lhs._digit--;
            }
            else
            {
                lhs._value = (ulong)(k);
            }
            lhs.Calc();
            return lhs;
        }

        static public BigNumber operator /(BigNumber lhs, int rhs)
        {
            var k = lhs._value / (float)rhs;
            if (k < 1f)
            {
                lhs._value = (ulong)(k * _unit);
                lhs._digit--;
            }
            else
            {
                lhs._value = (ulong)(k);
            }
            lhs.Calc();
            return lhs;
        }

        static public BigNumber operator /(BigNumber lhs, float rhs)
        {
            var k = lhs._value / (float)rhs;
            if (k < 1f)
            {
                lhs._value = (ulong)(k * _unit);
                lhs._digit--;
            }
            else
            {
                lhs._value = (ulong)(k);
            }
            lhs.Calc();
            return lhs;
        }

        static public bool operator <=(BigNumber lhs, BigNumber rhs)
        {
            return lhs < rhs || lhs == rhs;
        }

        static public bool operator >=(BigNumber lhs, BigNumber rhs)
        {
            return lhs > rhs || lhs == rhs;
        }

        static public bool operator <(BigNumber lhs, BigNumber rhs)
        {
            DigitBlance(ref lhs, ref rhs);
            if (lhs._digit < rhs._digit) return true;
            else if (lhs._digit == rhs._digit && lhs._value < rhs._value) return true;
            return false;
        }

        static public bool operator >(BigNumber lhs, BigNumber rhs)
        {
            DigitBlance(ref lhs, ref rhs);
            if (lhs._digit > rhs._digit) return true;
            else if (lhs._digit == rhs._digit && lhs._value > rhs._value) return true;
            return false;
        }

        static public bool operator ==(BigNumber lhs, BigNumber rhs)
        {
            DigitBlance(ref lhs, ref rhs);
            if (lhs._digit == rhs._digit && lhs._value == rhs._value) return true;
            return false;
        }

        static public bool operator !=(BigNumber lhs, BigNumber rhs)
        {
            DigitBlance(ref lhs, ref rhs);
            if (lhs._digit != rhs._digit || lhs._value != rhs._value) return true;
            return false;
        }

        static public BigNumber Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));
            if (TryParse(str, out var result))
                return result;
            throw new FormatException();
        }

        static public bool TryParse(string str, out BigNumber result)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            var match = Regex.Match(str, "^([0-9]+),([0-9]*)");
            if (match.Success)
            {
                result = new BigNumber(
                    long.Parse(match.Groups[1].Value),
                    string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : int.Parse(match.Groups[2].Value));
                return true;
            }

            match = Regex.Match(str, "^([0-9]+)([A-Z]*)");
            if (match.Success)
            {
                var value = long.Parse(match.Groups[1].Value);
                long digit = 0;
                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    int k = 0;
                    foreach (var d in match.Groups[2].Value.Reverse())
                    {
                        digit += (long)Math.Pow(26, k) * (d - 'A' + 1);
                        k++;
                    }
                }
                result = new BigNumber(value, digit * _unitDigit);
                return true;
            }
            result = Zero;
            return false;
        }

        static public BigNumber Max(BigNumber lhs, BigNumber rhs)
            => lhs > rhs ? lhs : rhs;

        static public BigNumber Min(BigNumber lhs, BigNumber rhs)
            => lhs < rhs ? lhs : rhs;

        static public float Ratio(BigNumber cur, BigNumber max)
        {
            DigitBlance(ref cur, ref max);
            if (cur.Digit > max.Digit) return 1f;
            else if (cur.Digit < max.Digit) return 0f;
            else return cur.Value / (float)max.Value;
        }

        private void Calc()
        {
            if (_value > 0)
            {
                while (_value >= _unitSq)
                {
                    _digit++;
                    _value /= _unit;
                }
                if (_digit < 0)
                {
                    _value = 0;
                    _digit = 0;
                }
            }
            else
            {
                _value = 0;
                _digit = 0;
            }
        }

        public override string ToString()
        {
            if (_value == 0) return "0";
            var digitChars = new Stack<char>();
            var d = _digit;
            var v = _value;
            var t = v / _unit;
            while (t >= 10)
            {
                v = t;
                d++;
                t = v / _unit;
            }
            if (Math.Log10(v) < 1 && d > 0)
            {
                v *= _unit;
                d--;
            }
            while (d > 0)
            {
                digitChars.Push((char)('A' + ((d - 1) % 26)));
                d = (d - 1) / 26;
            }
            return v.ToString() + new string(digitChars.ToArray());
        }

        public string ToDebug()
        {
            return ToString() + $"[{_value},{_digit},{RealDigit}]";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is not BigNumber)
                return false;

            return (BigNumber)obj == this;
        }

        public override int GetHashCode()
        {
            return (int)_value;
        }
    }

    static public class BigNumberExtension
    {
        static public BigNumber Sum<T>(this IEnumerable<T> values, Func<T, BigNumber> selector)
        {
            BigNumber ret = BigNumber.Zero;
            foreach (var v in values) ret += selector.Invoke(v);
            return ret;
        }
    }
}
