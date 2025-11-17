#pragma warning disable CS0612 // Obsoletion warnings

namespace Qdrant.Client.Grpc;

/// <summary>
/// Partial class for <see cref="VectorOutput"/> to add helper methods for extracting vector data.
/// </summary>
public partial class VectorOutput
{
	/// <summary>
	/// Gets the <see cref="DenseVector"/> from this <see cref="VectorOutput"/>.
	/// </summary>
	/// <returns>A <see cref="DenseVector"/> or null if no dense vector data is available</returns>
	public DenseVector? GetDenseVector()
	{
		if (Data.Count > 0)
		{
			return new DenseVector { Data = { Data } };
		}

		if (VectorCase == VectorOneofCase.Dense)
		{
			return Dense;
		}

		return null;
	}

	/// <summary>
	/// Gets the <see cref="SparseVector"/> from this <see cref="VectorOutput"/>.
	/// </summary>
	/// <returns>A <see cref="SparseVector"/> or null if no sparse vector data is available</returns>
	public SparseVector? GetSparseVector()
	{
		if (Data.Count > 0)
		{
			if (Indices == null)
			{
				return null;
			}

			return new SparseVector
			{
				Values = { Data },
				Indices = { Indices.Data }
			};
		}

		if (VectorCase == VectorOneofCase.Sparse)
		{
			return Sparse;
		}

		return null;
	}

	/// <summary>
	/// Gets the <see cref="MultiDenseVector"/> from this <see cref="VectorOutput"/>.
	/// </summary>
	/// <returns>A <see cref="MultiDenseVector"/> or null if no multi-dense vector data is available</returns>
	public MultiDenseVector? GetMultiVector()
	{
		if (Data.Count > 0)
		{
			var vectorsCount = VectorsCount;
			if (vectorsCount == 0)
			{
				return null;
			}

			var vectorSize = Data.Count / (int)vectorsCount;
			var vectors = new DenseVector[(int)vectorsCount];

			for (var i = 0; i < vectors.Length; i++)
			{
				var start = i * vectorSize;
				var end = start + vectorSize;
				vectors[i] = new DenseVector { Data = { Data.Skip(start).Take(vectorSize) } };
			}

			return new MultiDenseVector { Vectors = { vectors } };
		}

		if (VectorCase == VectorOneofCase.MultiDense)
		{
			return MultiDense;
		}

		return null;
	}
}
