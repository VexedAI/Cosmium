using System.Numerics;
using Cosmium.Engine.Infrastructure.Configuration;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Physics.Quantum.Constants;

namespace Cosmium.Engine.Infrastructure.Validation;

/// <summary>
/// Physics-specific validation for quantum mechanics and statistical physics calculations.
/// Ensures physical parameters are within realistic and computationally stable ranges.
/// </summary>
public static class PhysicsValidator
{
    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private static readonly PhysicsSettings Physics = EngineConfiguration.Instance.Physics;

    #region Fundamental Constants Validation

    /// <summary>
    /// Validates that a mass value is physically reasonable.
    /// </summary>
    /// <param name="mass">The mass value in kg.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowZero">Whether zero mass is allowed (for photons, etc.).</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateMass(double mass, string parameterName, bool allowZero = false)
    {
        var context = new { Parameter = parameterName, Mass = mass, AllowZero = allowZero };

        // Basic positivity check with appropriate tolerance for very small masses
        var positiveResult = ParameterValidator.ValidatePositive(mass, parameterName, allowZero, tolerance: 1e-40);
        if (!positiveResult.IsValid)
            return positiveResult;

        // Physical reasonableness: from electron mass to observable universe
        const double electronMass = 9.109e-31; // kg
        const double universeEstimatedMass = 1.5e53; // kg
        
        if (mass > 0 && mass < electronMass * 1e-10)
        {
            var warning = $"Mass '{parameterName}' ({mass} kg) is extremely small (< 10⁻⁴⁰ kg)";
            Logger.Warning(warning, context);
            // Don't fail, just warn
        }

        if (mass > universeEstimatedMass)
        {
            var error = $"Mass '{parameterName}' ({mass} kg) exceeds estimated universe mass";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that an energy value is physically reasonable.
    /// </summary>
    /// <param name="energy">The energy value in Joules.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowNegative">Whether negative energies are allowed.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateEnergy(double energy, string parameterName, bool allowNegative = false)
    {
        var context = new { Parameter = parameterName, Energy = energy, AllowNegative = allowNegative };

        var finiteResult = ParameterValidator.ValidateFiniteDouble(energy, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        if (!allowNegative && energy < 0)
        {
            var error = $"Energy '{parameterName}' cannot be negative in this context";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        // Physical reasonableness: from thermal energies to cosmic ray energies
        const double planckEnergy = 1.956e9; // J
        
        if (Math.Abs(energy) > planckEnergy)
        {
            var warning = $"Energy '{parameterName}' ({energy} J) exceeds Planck energy scale";
            Logger.Warning(warning, context);
            // Don't fail, but warn about extreme physics
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a length value is physically reasonable.
    /// </summary>
    /// <param name="length">The length value in meters.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateLength(double length, string parameterName)
    {
        var context = new { Parameter = parameterName, Length = length };

        var positiveResult = ParameterValidator.ValidatePositive(length, parameterName, allowZero: true);
        if (!positiveResult.IsValid)
            return positiveResult;

        // Physical reasonableness: from Planck length to observable universe
        const double planckLength = 1.616e-35; // m
        const double observableUniverseRadius = 4.65e26; // m
        
        if (length > 0 && length < planckLength * 1e-10)
        {
            var warning = $"Length '{parameterName}' ({length} m) is below meaningful physical scale";
            Logger.Warning(warning, context);
        }

        if (length > observableUniverseRadius)
        {
            var error = $"Length '{parameterName}' ({length} m) exceeds observable universe radius";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a time value is physically reasonable.
    /// </summary>
    /// <param name="time">The time value in seconds.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowNegative">Whether negative times are allowed.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateTime(double time, string parameterName, bool allowNegative = false)
    {
        var context = new { Parameter = parameterName, Time = time, AllowNegative = allowNegative };

        var finiteResult = ParameterValidator.ValidateFiniteDouble(time, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        if (!allowNegative && time < 0)
        {
            var error = $"Time '{parameterName}' cannot be negative in this context";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        // Physical reasonableness: from Planck time to age of universe
        const double planckTime = 5.391e-44; // s
        const double ageOfUniverse = 4.32e17; // s (13.7 billion years)
        
        if (Math.Abs(time) > 0 && Math.Abs(time) < planckTime * 1e-10)
        {
            var warning = $"Time '{parameterName}' ({time} s) is below meaningful physical scale";
            Logger.Warning(warning, context);
        }

        if (Math.Abs(time) > ageOfUniverse * 10) // Allow some headroom for cosmological simulations
        {
            var warning = $"Time '{parameterName}' ({time} s) exceeds reasonable cosmological timescales";
            Logger.Warning(warning, context);
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Temperature and Thermodynamics

    /// <summary>
    /// Validates that a temperature value is physically reasonable.
    /// </summary>
    /// <param name="temperature">The temperature in Kelvin.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowAbsoluteZero">Whether exactly 0K is allowed.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateTemperature(double temperature, string parameterName, bool allowAbsoluteZero = false)
    {
        var context = new { Parameter = parameterName, Temperature = temperature, AllowAbsoluteZero = allowAbsoluteZero };

        var finiteResult = ParameterValidator.ValidateFiniteDouble(temperature, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        if (temperature < 0)
        {
            var error = $"Temperature '{parameterName}' cannot be below absolute zero";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (!allowAbsoluteZero && temperature == 0)
        {
            var error = $"Temperature '{parameterName}' cannot be exactly absolute zero in this context";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        // Physical reasonableness checks
        const double cosmicMicrowaveBackground = 2.725; // K
        const double planckTemperature = 1.417e32; // K
        
        if (temperature > planckTemperature)
        {
            var error = $"Temperature '{parameterName}' ({temperature} K) exceeds Planck temperature";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (temperature > 0 && temperature < cosmicMicrowaveBackground * 1e-6)
        {
            var warning = $"Temperature '{parameterName}' ({temperature} K) is extremely low";
            Logger.Information(warning, context);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a heat capacity value is physically reasonable.
    /// </summary>
    /// <param name="heatCapacity">The heat capacity in J/K.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateHeatCapacity(double heatCapacity, string parameterName)
    {
        var positiveResult = ParameterValidator.ValidatePositive(heatCapacity, parameterName);
        if (!positiveResult.IsValid)
            return positiveResult;

        // Heat capacity should be reasonable for physical systems
        const double boltzmannConstant = PhysicsConstants.BoltzmannConstant;
        const double avogadroNumber = 6.022e23;
        
        // From single particle to macroscopic system
        if (heatCapacity < boltzmannConstant * 1e-10)
        {
            var warning = $"Heat capacity '{parameterName}' ({heatCapacity} J/K) is extremely small";
            Logger.Information(warning);
        }

        if (heatCapacity > boltzmannConstant * avogadroNumber * 1e6) // Much larger than ideal gas
        {
            var warning = $"Heat capacity '{parameterName}' ({heatCapacity} J/K) is unusually large";
            Logger.Information(warning);
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Quantum Mechanics

    /// <summary>
    /// Validates that a quantum number is physically reasonable.
    /// </summary>
    /// <param name="quantumNumber">The quantum number value.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="isHalfInteger">Whether half-integer values are allowed.</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateQuantumNumber(double quantumNumber, string parameterName, 
        bool isHalfInteger = false, double minValue = 0, double maxValue = 1000)
    {
        var context = new { Parameter = parameterName, Value = quantumNumber, IsHalfInteger = isHalfInteger };

        var finiteResult = ParameterValidator.ValidateFiniteDouble(quantumNumber, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        var rangeResult = ParameterValidator.ValidateRange(quantumNumber, parameterName, minValue, maxValue);
        if (!rangeResult.IsValid)
            return rangeResult;

        // Check for integer or half-integer constraint
        var tolerance = 1e-12;
        bool isInteger = Math.Abs(quantumNumber - Math.Round(quantumNumber)) < tolerance;
        bool isHalfInt = Math.Abs(quantumNumber - Math.Round(quantumNumber - 0.5) - 0.5) < tolerance;

        if (!isHalfInteger && !isInteger)
        {
            var error = $"Quantum number '{parameterName}' must be an integer";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (isHalfInteger && !isInteger && !isHalfInt)
        {
            var error = $"Quantum number '{parameterName}' must be an integer or half-integer";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a wave function is properly normalized and finite.
    /// </summary>
    /// <param name="wavefunction">The wave function values.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for normalization check.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateWaveFunction(Complex[]? wavefunction, string parameterName, double tolerance = 1e-12)
    {
        // Use quantum state validation which includes normalization
        return ParameterValidator.ValidateQuantumState(wavefunction, parameterName, tolerance);
    }

    /// <summary>
    /// Validates that a Hamiltonian matrix is Hermitian.
    /// </summary>
    /// <param name="hamiltonian">The Hamiltonian matrix.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for Hermiticity check.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateHamiltonian(Complex[,]? hamiltonian, string parameterName, double tolerance = 1e-12)
    {
        var matrixResult = ParameterValidator.ValidateFiniteComplexMatrix(hamiltonian, parameterName);
        if (!matrixResult.IsValid)
            return matrixResult;

        var squareResult = ParameterValidator.ValidateMatrix(hamiltonian, parameterName, requireSquare: true);
        if (!squareResult.IsValid)
            return squareResult;

        // Check Hermiticity: H† = H
        var n = hamiltonian!.GetLength(0);
        var context = new { Parameter = parameterName, Size = n, Tolerance = tolerance };

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                var hij = hamiltonian[i, j];
                var hji = hamiltonian[j, i];
                var hermitianConjugate = new Complex(hji.Real, -hji.Imaginary);

                var difference = hij - hermitianConjugate;
                if (difference.Magnitude > tolerance)
                {
                    var error = $"Hamiltonian '{parameterName}' is not Hermitian at ({i},{j}): H[{i},{j}] = {hij}, H†[{i},{j}] = {hermitianConjugate}";
                    Logger.Warning(error, context);
                    return ValidationResult.Failure(error);
                }
            }
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that eigenvalues are real for a Hermitian operator.
    /// </summary>
    /// <param name="eigenvalues">The eigenvalues to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for imaginary part check.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateRealEigenvalues(Complex[]? eigenvalues, string parameterName, double tolerance = 1e-12)
    {
        var arrayResult = ParameterValidator.ValidateFiniteComplexArray(eigenvalues, parameterName);
        if (!arrayResult.IsValid)
            return arrayResult;

        var context = new { Parameter = parameterName, Tolerance = tolerance };

        for (int i = 0; i < eigenvalues!.Length; i++)
        {
            if (Math.Abs(eigenvalues[i].Imaginary) > tolerance)
            {
                var error = $"Eigenvalue '{parameterName}[{i}]' must be real for Hermitian operator (imaginary part: {eigenvalues[i].Imaginary})";
                Logger.Warning(error, context);
                return ValidationResult.Failure(error);
            }
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Statistical Mechanics

    /// <summary>
    /// Validates that a partition function value is physically reasonable.
    /// </summary>
    /// <param name="partitionFunction">The partition function value.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidatePartitionFunction(double partitionFunction, string parameterName)
    {
        var context = new { Parameter = parameterName, Value = partitionFunction };

        var positiveResult = ParameterValidator.ValidatePositive(partitionFunction, parameterName);
        if (!positiveResult.IsValid)
            return positiveResult;

        // Partition function should be at least 1 (ground state)
        if (partitionFunction < 1.0 - 1e-12)
        {
            var error = $"Partition function '{parameterName}' cannot be less than 1";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        // Very large partition functions might indicate numerical issues
        if (partitionFunction > 1e100)
        {
            var warning = $"Partition function '{parameterName}' is very large ({partitionFunction}), check for numerical overflow";
            Logger.Warning(warning, context);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a chemical potential value is physically reasonable.
    /// </summary>
    /// <param name="chemicalPotential">The chemical potential in Joules.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="temperature">The temperature for context (optional).</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateChemicalPotential(double chemicalPotential, string parameterName, double? temperature = null)
    {
        var finiteResult = ParameterValidator.ValidateFiniteDouble(chemicalPotential, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        var context = new { Parameter = parameterName, Value = chemicalPotential, Temperature = temperature };

        // Chemical potential can be negative, but check physical reasonableness
        if (temperature.HasValue && temperature > 0)
        {
            var thermalEnergy = PhysicsConstants.BoltzmannConstant * temperature.Value;
            
            // Chemical potential much larger than thermal energy might be unusual
            if (Math.Abs(chemicalPotential) > thermalEnergy * 1000)
            {
                var warning = $"Chemical potential '{parameterName}' ({chemicalPotential} J) is much larger than thermal energy ({thermalEnergy} J)";
                Logger.Information(warning, context);
            }
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a density of states is non-negative and finite.
    /// </summary>
    /// <param name="densityOfStates">The density of states values.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateDensityOfStates(double[]? densityOfStates, string parameterName)
    {
        var arrayResult = ParameterValidator.ValidateFiniteDoubleArray(densityOfStates, parameterName);
        if (!arrayResult.IsValid)
            return arrayResult;

        for (int i = 0; i < densityOfStates!.Length; i++)
        {
            var positiveResult = ParameterValidator.ValidatePositive(densityOfStates[i], $"{parameterName}[{i}]", allowZero: true);
            if (!positiveResult.IsValid)
                return positiveResult;
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Electromagnetic Fields

    /// <summary>
    /// Validates that electric field components are physically reasonable.
    /// </summary>
    /// <param name="electricField">The electric field vector [Ex, Ey, Ez] in V/m.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateElectricField(double[]? electricField, string parameterName)
    {
        var arrayResult = ParameterValidator.ValidateArray(electricField, parameterName, exactLength: 3);
        if (!arrayResult.IsValid)
            return arrayResult;

        var finiteResult = ParameterValidator.ValidateFiniteDoubleArray(electricField, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        // Check field magnitude
        var magnitude = Math.Sqrt(electricField!.Sum(e => e * e));
        var context = new { Parameter = parameterName, Magnitude = magnitude };

        // Breakdown field for air is about 3e6 V/m
        // Nuclear fields can be much higher ~1e12 V/m
        const double airBreakdownField = 3e6; // V/m
        const double schwingersLimit = 1.32e18; // V/m (QED breakdown)

        if (magnitude > schwingersLimit)
        {
            var error = $"Electric field magnitude '{parameterName}' ({magnitude} V/m) exceeds Schwinger's limit";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (magnitude > airBreakdownField * 1000)
        {
            var warning = $"Electric field magnitude '{parameterName}' ({magnitude} V/m) is extremely high";
            Logger.Information(warning, context);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that magnetic field components are physically reasonable.
    /// </summary>
    /// <param name="magneticField">The magnetic field vector [Bx, By, Bz] in Tesla.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateMagneticField(double[]? magneticField, string parameterName)
    {
        var arrayResult = ParameterValidator.ValidateArray(magneticField, parameterName, exactLength: 3);
        if (!arrayResult.IsValid)
            return arrayResult;

        var finiteResult = ParameterValidator.ValidateFiniteDoubleArray(magneticField, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        // Check field magnitude
        var magnitude = Math.Sqrt(magneticField!.Sum(b => b * b));
        var context = new { Parameter = parameterName, Magnitude = magnitude };

        // Earth's field ~50 µT, lab magnets ~10 T, neutron stars ~1e8 T
        const double strongLabMagnet = 50; // T
        const double neutronStarField = 1e8; // T

        if (magnitude > neutronStarField * 1000)
        {
            var error = $"Magnetic field magnitude '{parameterName}' ({magnitude} T) exceeds realistic astrophysical values";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (magnitude > strongLabMagnet * 100)
        {
            var warning = $"Magnetic field magnitude '{parameterName}' ({magnitude} T) is extremely high";
            Logger.Information(warning, context);
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Composite Validation

    /// <summary>
    /// Validates a complete quantum system setup.
    /// </summary>
    /// <param name="hamiltonian">The system Hamiltonian.</param>
    /// <param name="initialState">The initial quantum state.</param>
    /// <param name="timeStep">The time evolution step.</param>
    /// <param name="totalTime">The total simulation time.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateQuantumSystem(Complex[,]? hamiltonian, Complex[]? initialState, double timeStep, double totalTime)
    {
        return ParameterValidator.ValidateAll(
            () => ValidateHamiltonian(hamiltonian, "hamiltonian"),
            () => ValidateWaveFunction(initialState, "initialState"),
            () => ValidateTime(timeStep, "timeStep"),
            () => ValidateTime(totalTime, "totalTime"),
            () => ParameterValidator.ValidateIf(timeStep > totalTime, 
                () => ValidationResult.Failure("Time step cannot be larger than total time"))
        );
    }

    /// <summary>
    /// Validates a thermal equilibrium system.
    /// </summary>
    /// <param name="temperature">The system temperature.</param>
    /// <param name="chemicalPotential">The chemical potential.</param>
    /// <param name="energyLevels">The available energy levels.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateThermalSystem(double temperature, double chemicalPotential, double[]? energyLevels)
    {
        return ParameterValidator.ValidateAll(
            () => ValidateTemperature(temperature, "temperature"),
            () => ValidateChemicalPotential(chemicalPotential, "chemicalPotential", temperature),
            () => ParameterValidator.ValidateFiniteDoubleArray(energyLevels, "energyLevels"),
            () => ValidateEnergySpectrum(energyLevels, "energyLevels")
        );
    }

    /// <summary>
    /// Validates an energy spectrum (eigenvalues should be real and finite).
    /// </summary>
    /// <param name="energyLevels">The energy spectrum.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateEnergySpectrum(double[]? energyLevels, string parameterName)
    {
        var arrayResult = ParameterValidator.ValidateFiniteDoubleArray(energyLevels, parameterName);
        if (!arrayResult.IsValid)
            return arrayResult;

        // Check for reasonable energy ordering (though not strictly required)
        for (int i = 0; i < energyLevels!.Length; i++)
        {
            var energyResult = ValidateEnergy(energyLevels[i], $"{parameterName}[{i}]", allowNegative: true);
            if (!energyResult.IsValid)
                return energyResult;
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Physical Unit Consistency

    /// <summary>
    /// Validates that a frequency value is physically reasonable.
    /// </summary>
    /// <param name="frequency">The frequency in Hz.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateFrequency(double frequency, string parameterName)
    {
        var positiveResult = ParameterValidator.ValidatePositive(frequency, parameterName, allowZero: true);
        if (!positiveResult.IsValid)
            return positiveResult;

        const double planckFrequency = 1.855e43; // Hz

        var context = new { Parameter = parameterName, Frequency = frequency };

        if (frequency > planckFrequency)
        {
            var error = $"Frequency '{parameterName}' ({frequency} Hz) exceeds Planck frequency";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a wavelength corresponds to the given frequency through c = λν.
    /// </summary>
    /// <param name="wavelength">The wavelength in meters.</param>
    /// <param name="frequency">The frequency in Hz.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for the consistency check.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateWavelengthFrequencyConsistency(double wavelength, double frequency, 
        string parameterName, double tolerance = 1e-12)
    {
        var lengthResult = ValidateLength(wavelength, $"{parameterName}.wavelength");
        if (!lengthResult.IsValid)
            return lengthResult;

        var freqResult = ValidateFrequency(frequency, $"{parameterName}.frequency");
        if (!freqResult.IsValid)
            return freqResult;

        // Check c = λν
        var speedOfLight = PhysicsConstants.SpeedOfLight;
        var expectedWavelength = speedOfLight / frequency;
        var relativeError = Math.Abs(wavelength - expectedWavelength) / expectedWavelength;

        var context = new { Parameter = parameterName, Wavelength = wavelength, Frequency = frequency, 
                           Expected = expectedWavelength, RelativeError = relativeError };

        if (relativeError > tolerance)
        {
            var error = $"Wavelength-frequency consistency check failed for '{parameterName}': λ = {wavelength} m, ν = {frequency} Hz, expected λ = {expectedWavelength} m";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    #endregion
}
