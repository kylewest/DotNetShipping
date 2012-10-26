using System;

namespace DotNetShipping
{
	/// <summary>
	/// 	Summary description for Package.
	/// </summary>
	public class Package
	{
		#region Fields

		public readonly decimal Height;
		public readonly decimal InsuredValue;
		public readonly decimal Length;
		public readonly int Ounces;
		public readonly int Pounds;
		public readonly decimal Weight;
		public readonly decimal Width;
		public string Container;
		public bool Machinable;
		public string Service;

		#endregion

		#region .ctor

		/// <summary>
		/// 	Creates a new package object.
		/// </summary>
		/// <param name = "length">The length of the package, in inches.</param>
		/// <param name = "width">The width of the package, in inches.</param>
		/// <param name = "height">The height of the package, in inches.</param>
		/// <param name = "weight">The weight of the package, in pounds.</param>
		/// <param name = "insuredValue">The insured-value of the package, in dollars.</param>
		public Package(int length, int width, int height, int weight, decimal insuredValue)
			: this(length, width, height, (decimal) weight, insuredValue)
		{
		}

		/// <summary>
		/// 	Creates a new package object.
		/// </summary>
		/// <param name = "length">The length of the package, in inches.</param>
		/// <param name = "width">The width of the package, in inches.</param>
		/// <param name = "height">The height of the package, in inches.</param>
		/// <param name = "weight">The weight of the package, in pounds.</param>
		/// <param name = "insuredValue">The insured-value of the package, in dollars.</param>
		public Package(decimal length, decimal width, decimal height, decimal weight, decimal insuredValue)
		{
			Length = length;
			Width = width;
			Height = height;
			Weight = weight;
			InsuredValue = insuredValue;
			Pounds = Convert.ToInt32(Weight - Weight%1);
			decimal tempWeight = weight*16;
			Ounces = Convert.ToInt32(Math.Ceiling((double) tempWeight - Pounds*16.0));
		}

		/// <summary>
		/// 	Creates a new package object with pounds and ounces specified.
		/// </summary>
		/// <param name = "length">The length of the package, in inches.</param>
		/// <param name = "width">The width of the package, in inches.</param>
		/// <param name = "height">The height of the package, in inches.</param>
		/// <param name = "pounds">Weight in pounds</param>
		/// <param name = "ounces">Weight in ounces, uses as in 8 pounds 5 ounces.</param>
		/// <param name = "insuredValue">The insured-value of the package, in dollars.</param>
		public Package(decimal length, decimal width, decimal height, int pounds, int ounces, decimal insuredValue)
		{
			Length = length;
			Width = width;
			Height = height;
			Weight = pounds + ounces/16;
			InsuredValue = insuredValue;
			Pounds = pounds;
			Ounces = ounces;
		}

		#endregion
	}
}