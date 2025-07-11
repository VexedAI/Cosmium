using System;
using System.Text;
using System.Globalization;

namespace Cosmium.Engine.Physics.Mathematics;

/// <summary>
/// Represents a matrix with complex elements optimized for quantum mechanics calculations.
/// Provides essential matrix operations for quantum state vectors, operators, and transformations.
/// </summary>
public class Matrix : IEquatable<Matrix>
{
    #region Fields and Properties

    private readonly Complex[,] _elements;

    /// <summary>
    /// Gets the number of rows in the matrix.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the number of columns in the matrix.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets or sets the element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0-based).</param>
    /// <param name="col">The column index (0-based).</param>
    /// <returns>The complex element at the specified position.</returns>
    public Complex this[int row, int col]
    {
        get
        {
            ValidateIndices(row, col);
            return _elements[row, col];
        }
        set
        {
            ValidateIndices(row, col);
            _elements[row, col] = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this matrix is square.
    /// </summary>
    public bool IsSquare => Rows == Columns;

    /// <summary>
    /// Gets a value indicating whether this matrix is a vector (single column).
    /// </summary>
    public bool IsVector => Columns == 1;

    /// <summary>
    /// Gets a value indicating whether this matrix is a row vector (single row).
    /// </summary>
    public bool IsRowVector => Rows == 1;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new matrix with the specified dimensions.
    /// </summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    public Matrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Matrix dimensions must be positive.");

        Rows = rows;
        Columns = columns;
        _elements = new Complex[rows, columns];
    }

    /// <summary>
    /// Initializes a new matrix from a 2D array of complex numbers.
    /// </summary>
    /// <param name="elements">The 2D array of complex elements.</param>
    public Matrix(Complex[,] elements)
    {
        if (elements == null)
            throw new ArgumentNullException(nameof(elements));

        Rows = elements.GetLength(0);
        Columns = elements.GetLength(1);
        _elements = new Complex[Rows, Columns];

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                _elements[i, j] = elements[i, j];
            }
        }
    }

    /// <summary>
    /// Initializes a new matrix from a 2D array of real numbers.
    /// </summary>
    /// <param name="elements">The 2D array of real elements.</param>
    public Matrix(double[,] elements)
    {
        if (elements == null)
            throw new ArgumentNullException(nameof(elements));

        Rows = elements.GetLength(0);
        Columns = elements.GetLength(1);
        _elements = new Complex[Rows, Columns];

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                _elements[i, j] = new Complex(elements[i, j]);
            }
        }
    }

    /// <summary>
    /// Initializes a new matrix from a jagged array of complex numbers.
    /// </summary>
    /// <param name="elements">The jagged array of complex elements.</param>
    public Matrix(Complex[][] elements)
    {
        if (elements == null || elements.Length == 0)
            throw new ArgumentException("Elements array cannot be null or empty.");

        Rows = elements.Length;
        Columns = elements[0].Length;

        // Validate that all rows have the same length
        for (int i = 1; i < Rows; i++)
        {
            if (elements[i].Length != Columns)
                throw new ArgumentException("All rows must have the same number of columns.");
        }

        _elements = new Complex[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                _elements[i, j] = elements[i][j];
            }
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates an identity matrix of the specified size.
    /// </summary>
    /// <param name="size">The size of the identity matrix.</param>
    /// <returns>An identity matrix.</returns>
    public static Matrix Identity(int size)
    {
        if (size <= 0)
            throw new ArgumentException("Size must be positive.");

        var matrix = new Matrix(size, size);
        for (int i = 0; i < size; i++)
        {
            matrix[i, i] = Complex.One;
        }
        return matrix;
    }

    /// <summary>
    /// Creates a zero matrix of the specified dimensions.
    /// </summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    /// <returns>A zero matrix.</returns>
    public static Matrix Zero(int rows, int columns)
    {
        return new Matrix(rows, columns);
    }

    /// <summary>
    /// Creates a matrix with all elements set to the specified value.
    /// </summary>
    /// <param name="rows">The number of rows.</param>
    /// <param name="columns">The number of columns.</param>
    /// <param name="value">The value for all elements.</param>
    /// <returns>A matrix with all elements set to the specified value.</returns>
    public static Matrix Fill(int rows, int columns, Complex value)
    {
        var matrix = new Matrix(rows, columns);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                matrix[i, j] = value;
            }
        }
        return matrix;
    }

    /// <summary>
    /// Creates a diagonal matrix with the specified diagonal elements.
    /// </summary>
    /// <param name="diagonalElements">The diagonal elements.</param>
    /// <returns>A diagonal matrix.</returns>
    public static Matrix Diagonal(params Complex[] diagonalElements)
    {
        if (diagonalElements == null || diagonalElements.Length == 0)
            throw new ArgumentException("Diagonal elements cannot be null or empty.");

        int size = diagonalElements.Length;
        var matrix = new Matrix(size, size);
        for (int i = 0; i < size; i++)
        {
            matrix[i, i] = diagonalElements[i];
        }
        return matrix;
    }

    /// <summary>
    /// Creates a column vector from an array of complex numbers.
    /// </summary>
    /// <param name="elements">The vector elements.</param>
    /// <returns>A column vector matrix.</returns>
    public static Matrix ColumnVector(params Complex[] elements)
    {
        if (elements == null || elements.Length == 0)
            throw new ArgumentException("Elements cannot be null or empty.");

        var matrix = new Matrix(elements.Length, 1);
        for (int i = 0; i < elements.Length; i++)
        {
            matrix[i, 0] = elements[i];
        }
        return matrix;
    }

    /// <summary>
    /// Creates a row vector from an array of complex numbers.
    /// </summary>
    /// <param name="elements">The vector elements.</param>
    /// <returns>A row vector matrix.</returns>
    public static Matrix RowVector(params Complex[] elements)
    {
        if (elements == null || elements.Length == 0)
            throw new ArgumentException("Elements cannot be null or empty.");

        var matrix = new Matrix(1, elements.Length);
        for (int j = 0; j < elements.Length; j++)
        {
            matrix[0, j] = elements[j];
        }
        return matrix;
    }

    #endregion

    #region Arithmetic Operations

    /// <summary>
    /// Adds two matrices.
    /// </summary>
    public static Matrix operator +(Matrix left, Matrix right)
    {
        if (left == null || right == null)
            throw new ArgumentNullException("Matrices cannot be null.");

        if (left.Rows != right.Rows || left.Columns != right.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for addition.");

        var result = new Matrix(left.Rows, left.Columns);
        for (int i = 0; i < left.Rows; i++)
        {
            for (int j = 0; j < left.Columns; j++)
            {
                result[i, j] = left[i, j] + right[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Subtracts two matrices.
    /// </summary>
    public static Matrix operator -(Matrix left, Matrix right)
    {
        if (left == null || right == null)
            throw new ArgumentNullException("Matrices cannot be null.");

        if (left.Rows != right.Rows || left.Columns != right.Columns)
            throw new ArgumentException("Matrices must have the same dimensions for subtraction.");

        var result = new Matrix(left.Rows, left.Columns);
        for (int i = 0; i < left.Rows; i++)
        {
            for (int j = 0; j < left.Columns; j++)
            {
                result[i, j] = left[i, j] - right[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    public static Matrix operator *(Matrix left, Matrix right)
    {
        if (left == null || right == null)
            throw new ArgumentNullException("Matrices cannot be null.");

        if (left.Columns != right.Rows)
            throw new ArgumentException("Left matrix columns must equal right matrix rows for multiplication.");

        var result = new Matrix(left.Rows, right.Columns);
        for (int i = 0; i < left.Rows; i++)
        {
            for (int j = 0; j < right.Columns; j++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < left.Columns; k++)
                {
                    sum += left[i, k] * right[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// Multiplies a matrix by a scalar.
    /// </summary>
    public static Matrix operator *(Matrix matrix, Complex scalar)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        var result = new Matrix(matrix.Rows, matrix.Columns);
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i, j] = matrix[i, j] * scalar;
            }
        }
        return result;
    }

    /// <summary>
    /// Multiplies a scalar by a matrix.
    /// </summary>
    public static Matrix operator *(Complex scalar, Matrix matrix)
    {
        return matrix * scalar;
    }

    /// <summary>
    /// Divides a matrix by a scalar.
    /// </summary>
    public static Matrix operator /(Matrix matrix, Complex scalar)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        if (scalar == Complex.Zero)
            throw new DivideByZeroException("Cannot divide matrix by zero.");

        return matrix * (Complex.One / scalar);
    }

    /// <summary>
    /// Negates a matrix.
    /// </summary>
    public static Matrix operator -(Matrix matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        return matrix * (-Complex.One);
    }

    #endregion

    #region Matrix Operations

    /// <summary>
    /// Calculates the transpose of the matrix.
    /// </summary>
    /// <returns>The transposed matrix.</returns>
    public Matrix Transpose()
    {
        var result = new Matrix(Columns, Rows);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[j, i] = this[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Calculates the conjugate transpose (Hermitian conjugate) of the matrix.
    /// Essential for quantum mechanics calculations.
    /// </summary>
    /// <returns>The conjugate transpose matrix.</returns>
    public Matrix ConjugateTranspose()
    {
        var result = new Matrix(Columns, Rows);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[j, i] = this[i, j].Conjugate;
            }
        }
        return result;
    }

    /// <summary>
    /// Alias for ConjugateTranspose, commonly used in physics notation.
    /// </summary>
    /// <returns>The Hermitian conjugate matrix.</returns>
    public Matrix Dagger() => ConjugateTranspose();

    /// <summary>
    /// Calculates the trace of the matrix (sum of diagonal elements).
    /// </summary>
    /// <returns>The trace of the matrix.</returns>
    public Complex Trace()
    {
        if (!IsSquare)
            throw new InvalidOperationException("Trace is only defined for square matrices.");

        Complex trace = Complex.Zero;
        for (int i = 0; i < Rows; i++)
        {
            trace += this[i, i];
        }
        return trace;
    }

    /// <summary>
    /// Calculates the Frobenius norm of the matrix.
    /// </summary>
    /// <returns>The Frobenius norm.</returns>
    public double FrobeniusNorm()
    {
        double sum = 0.0;
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sum += this[i, j].MagnitudeSquared;
            }
        }
        return Math.Sqrt(sum);
    }

    /// <summary>
    /// Checks if the matrix is Hermitian (self-adjoint).
    /// </summary>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the matrix is Hermitian.</returns>
    public bool IsHermitian(double tolerance = 1e-10)
    {
        if (!IsSquare)
            return false;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (!this[i, j].Equals(this[j, i].Conjugate, tolerance))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if the matrix is unitary.
    /// </summary>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the matrix is unitary.</returns>
    public bool IsUnitary(double tolerance = 1e-10)
    {
        if (!IsSquare)
            return false;

        var product = this * ConjugateTranspose();
        var identity = Identity(Rows);

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (!product[i, j].Equals(identity[i, j], tolerance))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Extracts a submatrix from the current matrix.
    /// </summary>
    /// <param name="startRow">The starting row index.</param>
    /// <param name="startCol">The starting column index.</param>
    /// <param name="rowCount">The number of rows to extract.</param>
    /// <param name="colCount">The number of columns to extract.</param>
    /// <returns>The extracted submatrix.</returns>
    public Matrix Submatrix(int startRow, int startCol, int rowCount, int colCount)
    {
        if (startRow < 0 || startCol < 0 || rowCount <= 0 || colCount <= 0)
            throw new ArgumentException("Invalid submatrix parameters.");

        if (startRow + rowCount > Rows || startCol + colCount > Columns)
            throw new ArgumentException("Submatrix extends beyond matrix bounds.");

        var result = new Matrix(rowCount, colCount);
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                result[i, j] = this[startRow + i, startCol + j];
            }
        }
        return result;
    }

    #endregion

    #region Quantum-Specific Operations

    /// <summary>
    /// Calculates the inner product of two quantum state vectors.
    /// For vectors |ψ⟩ and |φ⟩, returns ⟨ψ|φ⟩.
    /// </summary>
    /// <param name="other">The other state vector.</param>
    /// <returns>The inner product.</returns>
    public Complex InnerProduct(Matrix other)
    {
        if (!IsVector || !other.IsVector)
            throw new InvalidOperationException("Inner product is only defined for vectors.");

        if (Rows != other.Rows)
            throw new ArgumentException("Vectors must have the same dimension.");

        var result = ConjugateTranspose() * other;
        return result[0, 0];
    }

    /// <summary>
    /// Calculates the outer product of two quantum state vectors.
    /// For vectors |ψ⟩ and |φ⟩, returns |ψ⟩⟨φ|.
    /// </summary>
    /// <param name="other">The other state vector.</param>
    /// <returns>The outer product matrix.</returns>
    public Matrix OuterProduct(Matrix other)
    {
        if (!IsVector || !other.IsVector)
            throw new InvalidOperationException("Outer product is only defined for vectors.");

        return this * other.ConjugateTranspose();
    }

    /// <summary>
    /// Normalizes a quantum state vector to unit length.
    /// </summary>
    /// <returns>The normalized state vector.</returns>
    public Matrix Normalize()
    {
        if (!IsVector)
            throw new InvalidOperationException("Normalization is only defined for vectors.");

        double norm = FrobeniusNorm();
        if (norm < double.Epsilon)
            throw new InvalidOperationException("Cannot normalize zero vector.");

        return this / norm;
    }

    /// <summary>
    /// Calculates the expectation value of an observable for a given state.
    /// Returns ⟨ψ|A|ψ⟩ for operator A and state |ψ⟩.
    /// </summary>
    /// <param name="observable">The observable operator.</param>
    /// <returns>The expectation value.</returns>
    public Complex ExpectationValue(Matrix observable)
    {
        if (!IsVector)
            throw new InvalidOperationException("Expectation value requires a state vector.");

        if (!observable.IsSquare || observable.Rows != Rows)
            throw new ArgumentException("Observable must be a square matrix with same dimension as state.");

        var result = ConjugateTranspose() * observable * this;
        return result[0, 0];
    }

    /// <summary>
    /// Checks if the matrix represents a valid quantum state (normalized vector).
    /// </summary>
    /// <param name="tolerance">The tolerance for normalization check.</param>
    /// <returns>True if the matrix is a valid quantum state.</returns>
    public bool IsValidQuantumState(double tolerance = 1e-10)
    {
        if (!IsVector)
            return false;

        double norm = FrobeniusNorm();
        return Math.Abs(norm - 1.0) < tolerance;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Validates that the specified indices are within the matrix bounds.
    /// </summary>
    private void ValidateIndices(int row, int col)
    {
        if (row < 0 || row >= Rows)
            throw new IndexOutOfRangeException($"Row index {row} is out of range [0, {Rows - 1}].");

        if (col < 0 || col >= Columns)
            throw new IndexOutOfRangeException($"Column index {col} is out of range [0, {Columns - 1}].");
    }

    /// <summary>
    /// Creates a copy of this matrix.
    /// </summary>
    /// <returns>A copy of the matrix.</returns>
    public Matrix Clone()
    {
        var clone = new Matrix(Rows, Columns);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                clone[i, j] = this[i, j];
            }
        }
        return clone;
    }

    /// <summary>
    /// Gets a row from the matrix as a new matrix.
    /// </summary>
    /// <param name="rowIndex">The row index.</param>
    /// <returns>A row vector matrix.</returns>
    public Matrix GetRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= Rows)
            throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range.");

        var row = new Matrix(1, Columns);
        for (int j = 0; j < Columns; j++)
        {
            row[0, j] = this[rowIndex, j];
        }
        return row;
    }

    /// <summary>
    /// Gets a column from the matrix as a new matrix.
    /// </summary>
    /// <param name="colIndex">The column index.</param>
    /// <returns>A column vector matrix.</returns>
    public Matrix GetColumn(int colIndex)
    {
        if (colIndex < 0 || colIndex >= Columns)
            throw new IndexOutOfRangeException($"Column index {colIndex} is out of range.");

        var column = new Matrix(Rows, 1);
        for (int i = 0; i < Rows; i++)
        {
            column[i, 0] = this[i, colIndex];
        }
        return column;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Determines whether two matrices are equal.
    /// </summary>
    public static bool operator ==(Matrix? left, Matrix? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two matrices are not equal.
    /// </summary>
    public static bool operator !=(Matrix? left, Matrix? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether this matrix is equal to another.
    /// </summary>
    public bool Equals(Matrix? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Rows != other.Rows || Columns != other.Columns)
            return false;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (!this[i, j].Equals(other[i, j]))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether this matrix is approximately equal to another within a tolerance.
    /// </summary>
    /// <param name="other">The other matrix.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the matrices are approximately equal.</returns>
    public bool Equals(Matrix? other, double tolerance)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Rows != other.Rows || Columns != other.Columns)
            return false;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (!this[i, j].Equals(other[i, j], tolerance))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether this instance is equal to a specified object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Matrix other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Rows);
        hash.Add(Columns);

        // Include some elements in the hash, but not all to avoid performance issues
        int step = Math.Max(1, (Rows * Columns) / 16);
        for (int i = 0; i < Rows * Columns; i += step)
        {
            int row = i / Columns;
            int col = i % Columns;
            hash.Add(_elements[row, col]);
        }

        return hash.ToHashCode();
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the matrix.
    /// </summary>
    public override string ToString()
    {
        return ToString("G");
    }

    /// <summary>
    /// Returns a string representation of the matrix with specified formatting.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <returns>A formatted string representation.</returns>
    public string ToString(string format)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Matrix {Rows}×{Columns}:");

        for (int i = 0; i < Rows; i++)
        {
            sb.Append("[ ");
            for (int j = 0; j < Columns; j++)
            {
                sb.Append(this[i, j].ToString(format, CultureInfo.CurrentCulture));
                if (j < Columns - 1)
                    sb.Append(", ");
            }
            sb.AppendLine(" ]");
        }

        return sb.ToString();
    }

    #endregion
}
