﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Math.Gmp
{
    public class BigFloat : IDisposable
    {
        public static int RawStructSize => Marshal.SizeOf<Mpf_t>();

        public static uint DefaultPrecision
        {
            get => GmpNative.__gmpf_get_default_prec();
            set => GmpNative.__gmpf_set_default_prec(value);
        }

        public Mpf_t Raw = new();
        private bool _disposed = false;

        #region Initialization functions

        public unsafe BigFloat()
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpNative.__gmpf_init((IntPtr)ptr);
            }
        }

        public BigFloat(Mpf_t raw)
        {
            Raw = raw;
        }

        public unsafe BigFloat(uint precision)
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                if (precision == 0)
                {
                    GmpNative.__gmpf_init((IntPtr)ptr);
                }
                else
                {
                    GmpNative.__gmpf_init2((IntPtr)ptr, precision);
                }
            }
        }
        #endregion

        #region Combined Initialization and Assignment Functions

        public unsafe static BigFloat From(int val)
        {
            Mpf_t raw = new();
            Mpf_t* ptr = &raw;
            GmpNative.__gmpf_init_set_si((IntPtr)ptr, val);
            return new BigFloat(raw);
        }

        public unsafe static BigFloat From(uint val)
        {
            Mpf_t raw = new();
            Mpf_t* ptr = &raw;
            GmpNative.__gmpf_init_set_ui((IntPtr)ptr, val);
            return new BigFloat(raw);
        }

        public unsafe static BigFloat From(double val)
        {
            Mpf_t raw = new();
            Mpf_t* ptr = &raw;
            GmpNative.__gmpf_init_set_d((IntPtr)ptr, val);
            return new BigFloat(raw);
        }

        public unsafe static BigFloat Parse(string val, int valBase = 10)
        {
            Mpf_t raw = new();
            Mpf_t* ptr = &raw;
            byte[] valBytes = Encoding.UTF8.GetBytes(val);
            fixed (byte* pval = valBytes)
            {
                int ret = GmpNative.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
                if (ret != 0)
                {
                    GmpNative.__gmpf_clear((IntPtr)ptr);
                    throw new FormatException($"Failed to parse {val}, base={valBase} to BigFloat, __gmpf_init_set_str returns {ret}");
                }
            }
            return new BigFloat(raw);
        }

        public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out BigFloat result, int valBase = 10)
        {
            Mpf_t raw = new();
            Mpf_t* ptr = &raw;
            byte[] valBytes = Encoding.UTF8.GetBytes(val);
            fixed (byte* pval = valBytes)
            {
                int rt = GmpNative.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
                if (rt != 0)
                {
                    GmpNative.__gmpf_clear((IntPtr)ptr);
                    result = null;
                    return false;
                }
                else
                {
                    result = new BigFloat(raw);
                    return true;
                }
            }
        }

        public unsafe uint Precision
        {
            get
            {
                fixed (Mpf_t* ptr = &Raw)
                {
                    return GmpNative.__gmpf_get_prec((IntPtr)ptr);
                }
            }
            set
            {
                fixed (Mpf_t* ptr = &Raw)
                {
                    GmpNative.__gmpf_set_prec((IntPtr)ptr, value);
                }
            }
        }

        [Obsolete("use Precision")]
        public unsafe void SetRawPrecision(uint value)
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpNative.__gmpf_set_prec_raw((IntPtr)ptr, value);
            }
        }

        private unsafe void Clear()
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpNative.__gmpf_clear((IntPtr)ptr);
            }
        }
        #endregion

        #region Assignment functions
        public unsafe void Assign(BigFloat op)
        {
            fixed (Mpf_t* pthis = &Raw)
            fixed (Mpf_t* pthat = &op.Raw)
            {
                GmpNative.__gmpf_set((IntPtr)pthis, (IntPtr)pthat);
            }
        }

        public unsafe void Assign(uint op)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                GmpNative.__gmpf_set_ui((IntPtr)pthis, op);
            }
        }

        public unsafe void Assign(int op)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                GmpNative.__gmpf_set_si((IntPtr)pthis, op);
            }
        }

        public unsafe void Assign(double op)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                GmpNative.__gmpf_set_d((IntPtr)pthis, op);
            }
        }

        public unsafe void Assign(BigInteger op)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                // TODO: GmpNative.__gmpf_set_z((IntPtr)pthis, op);
                throw new NotImplementedException();
            }
        }

        public unsafe void Assign(BigRational op)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                // TODO: GmpNative.__gmpf_set_q((IntPtr)pthis, op);
                throw new NotImplementedException();
            }
        }

        public unsafe void Assign(string op, int opBase = 10)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                byte[] opBytes = Encoding.UTF8.GetBytes(op);
                fixed (byte* opBytesPtr = opBytes)
                {
                    int ret = GmpNative.__gmpf_set_str((IntPtr)pthis, (IntPtr)opBytesPtr, opBase);
                    if (ret != 0)
                    {
                        throw new FormatException($"Failed to parse {op}, base={opBase} to BigFloat, __gmpf_set_str returns {ret}");
                    }
                }
            }
        }

        public unsafe static void Swap(BigFloat op1, BigFloat op2)
        {
            fixed (Mpf_t* pthis = &op1.Raw)
            fixed (Mpf_t* pthat = &op2.Raw)
            {
                GmpNative.__gmpf_swap((IntPtr)pthis, (IntPtr)pthat);
            }
        }
        #endregion

        #region Conversion Functions
        public unsafe double ToDouble()
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpNative.__gmpf_get_d((IntPtr)ptr);
            }
        }

        public static explicit operator double(BigFloat op) => op.ToDouble();

        public unsafe ExpDouble ToExpDouble()
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                int exp;
                double val = GmpNative.__gmpf_get_d_2exp((IntPtr)ptr, (IntPtr)(&exp));
                return new ExpDouble(exp, val);
            }
        }

        public unsafe int ToInt32()
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpNative.__gmpf_get_si((IntPtr)ptr);
            }
        }

        public static explicit operator int(BigFloat op) => op.ToInt32();

        public unsafe uint ToUInt32()
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpNative.__gmpf_get_ui((IntPtr)ptr);
            }
        }

        public static explicit operator uint(BigFloat op) => op.ToUInt32();

        public unsafe override string? ToString() => ToString(@base: 10);

        public unsafe string? ToString(int @base = 10)
        {
            const nint srcptr = 0;
            const int digits = 0;
            fixed (Mpf_t* ptr = &Raw)
            {
                int exp;
                IntPtr ret = default;
                try
                {
                    ret = GmpNative.__gmpf_get_str(srcptr, (IntPtr)(&exp), @base, digits, (IntPtr)ptr);
                    string? result = Marshal.PtrToStringUTF8(ret);
                    if (result == null) return null;

                    return (result.Length - exp) switch
                    {
                        > 0 => result[..exp] + "." + result[exp..],
                        0 => result[..exp],
                        var x => result + new string('0', -x),
                    };
                }
                finally
                {
                    if (ret != IntPtr.Zero)
                    {
                        GmpMemory.Free(ret);
                    }
                }
            }
        }

        #endregion

        #region Arithmetic Functions
        #region Arithmetic Functions - Operators
        public static unsafe BigFloat operator +(BigFloat op1, BigFloat op2) => Add(op1, op2);

        public static unsafe BigFloat operator +(BigFloat op1, uint op2) => Add(op1, op2);

        public static unsafe BigFloat operator -(BigFloat op1, BigFloat op2) => Subtract(op1, op2);

        public static unsafe BigFloat operator -(BigFloat op1, uint op2) => Subtract(op1, op2);

        public static unsafe BigFloat operator -(uint op1, BigFloat op2) => Subtract(op1, op2);

        public static unsafe BigFloat operator *(BigFloat op1, BigFloat op2) => Subtract(op1, op2);

        public static unsafe BigFloat operator *(BigFloat op1, uint op2) => Subtract(op1, op2);

        public static unsafe BigFloat operator /(BigFloat op1, BigFloat op2) => Divide(op1, op2);

        public static unsafe BigFloat operator /(BigFloat op1, uint op2) => Divide(op1, op2);

        public static unsafe BigFloat operator /(uint op1, BigFloat op2) => Divide(op1, op2);

        public static unsafe BigFloat operator ^(BigFloat op1, uint op2) => Power(op1, op2);

        public static unsafe BigFloat operator -(BigFloat op1) => Negate(op1);

        #endregion

        #region Arithmetic Functions - Raw inplace functions
        public static unsafe void AddInplace(BigFloat rop, BigFloat op1, BigFloat op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            fixed (Mpf_t* pop2 = &op2.Raw)
            {
                GmpNative.__gmpf_add((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
            }
        }

        public static unsafe void AddInplace(BigFloat rop, BigFloat op1, uint op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_add_ui((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        public static unsafe void SubtractInplace(BigFloat rop, BigFloat op1, BigFloat op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            fixed (Mpf_t* pop2 = &op2.Raw)
            {
                GmpNative.__gmpf_sub((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
            }
        }

        public static unsafe void SubtractInplace(BigFloat rop, BigFloat op1, uint op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_sub_ui((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        public static unsafe void SubtractInplace(BigFloat rop, uint op1, BigFloat op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop2 = &op2.Raw)
            {
                GmpNative.__gmpf_ui_sub((IntPtr)prop, op1, (IntPtr)pop2);
            }
        }

        public static unsafe void MultiplyInplace(BigFloat rop, BigFloat op1, BigFloat op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            fixed (Mpf_t* pop2 = &op2.Raw)
            {
                GmpNative.__gmpf_mul((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
            }
        }

        public static unsafe void MultiplyInplace(BigFloat rop, BigFloat op1, uint op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_mul_ui((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        public static unsafe void DivideInplace(BigFloat rop, BigFloat op1, BigFloat op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            fixed (Mpf_t* pop2 = &op2.Raw)
            {
                GmpNative.__gmpf_div((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
            }
        }

        public static unsafe void DivideInplace(BigFloat rop, BigFloat op1, uint op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_div_ui((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        public static unsafe void DivideInplace(BigFloat rop, uint op1, BigFloat op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop2 = &op2.Raw)
            {
                GmpNative.__gmpf_ui_div((IntPtr)prop, op1, (IntPtr)pop2);
            }
        }

        public static unsafe void PowerInplace(BigFloat rop, BigFloat op1, uint op2)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_pow_ui((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        public static unsafe void NegateInplace(BigFloat rop, BigFloat op1)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            {
                GmpNative.__gmpf_neg((IntPtr)prop, (IntPtr)prop);
            }
        }

        public static unsafe void SqrtInplace(BigFloat rop, BigFloat op)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop = &op.Raw)
            {
                GmpNative.__gmpf_sqrt((IntPtr)prop, (IntPtr)pop);
            }
        }

        public static unsafe void SqrtInplace(BigFloat rop, uint op, uint precision = 0)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            {
                GmpNative.__gmpf_sqrt_ui((IntPtr)prop, op);
            }
        }

        public static unsafe void AbsInplace(BigFloat rop, BigFloat op, uint precision = 0)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop = &op.Raw)
            {
                GmpNative.__gmpf_abs((IntPtr)prop, (IntPtr)pop);
            }
        }

        /// <summary>
        /// op1 * Math.Pow(2, op2)
        /// </summary>
        public static unsafe void Mul2ExpInplace(BigFloat rop, BigFloat op1, uint op2, uint precision = 0)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_mul_2exp((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        /// <summary>
        /// op1 / Math.Pow(2, op2)
        /// </summary>
        public static unsafe void Div2ExpInplace(BigFloat rop, BigFloat op1, uint op2, uint precision = 0)
        {
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop1 = &op1.Raw)
            {
                GmpNative.__gmpf_div_2exp((IntPtr)prop, (IntPtr)pop1, op2);
            }
        }

        #endregion

        #region Arithmetic Functions - Easier functions

        public static unsafe BigFloat Add(BigFloat op1, BigFloat op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            AddInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Add(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            AddInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Subtract(BigFloat op1, BigFloat op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            AddInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Subtract(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            SubtractInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Subtract(uint op1, BigFloat op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            SubtractInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Multiple(BigFloat op1, BigFloat op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            MultiplyInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Multiple(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            MultiplyInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Divide(BigFloat op1, BigFloat op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            DivideInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Divide(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            DivideInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Divide(uint op1, BigFloat op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            DivideInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Power(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            PowerInplace(rop, op1, op2);
            return rop;
        }

        public static unsafe BigFloat Negate(BigFloat op1, uint precision = 0)
        {
            BigFloat rop = new(precision);
            NegateInplace(rop, op1);
            return rop;
        }

        public static unsafe BigFloat Sqrt(BigFloat op, uint precision = 0)
        {
            BigFloat rop = new(precision);
            SqrtInplace(rop, op);
            return rop;
        }

        public static unsafe BigFloat Sqrt(uint op, uint precision = 0)
        {
            BigFloat rop = new(precision);
            SqrtInplace(rop, op);
            return rop;
        }

        public static unsafe BigFloat Abs(BigFloat op, uint precision = 0)
        {
            BigFloat rop = new(precision);
            AbsInplace(rop, op);
            return rop;
        }

        /// <summary>
        /// op1 * Math.Pow(2, op2)
        /// </summary>
        public static unsafe BigFloat Mul2Exp(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            Mul2ExpInplace(rop, op1, op2);
            return rop;
        }

        /// <summary>
        /// op1 / Math.Pow(2, op2)
        /// </summary>
        public static unsafe BigFloat Div2Exp(BigFloat op1, uint op2, uint precision = 0)
        {
            BigFloat rop = new(precision);
            Div2ExpInplace(rop, op1, op2);
            return rop;
        }
        #endregion
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                Clear();
                _disposed = true;
            }
        }

        ~BigFloat()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public struct Mpf_t
    {
        public int Precision;
        public int Size;
        public int Exponent;
        public IntPtr Limbs;
    }

    public record struct ExpDouble(int Exp, double Value);
}
