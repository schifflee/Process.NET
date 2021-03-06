﻿using System;
using System.Text;
using Process.NET.Marshaling;
using Process.NET.Threads;

namespace Process.NET.Assembly
{
  /// <summary>
  ///   Class representing a transaction where the user can insert mnemonics. The code is
  ///   then executed when the object is disposed.
  /// </summary>
  public class AssemblyTransaction : IDisposable, IAssemblyTransaction
  {
    #region Properties & Fields - Non-Public

    private readonly IAssemblyFactory _assemblyFactory;
    private readonly IRemoteThread    _executingThread;

    /// <summary>The exit code of the thread created to execute the assembly code.</summary>
    protected IntPtr _exitCode;

    /// <summary>The builder contains all the mnemonics inserted by the user.</summary>
    protected StringBuilder _mnemonics;

    #endregion




    #region Constructors

    /// <summary>Initializes a new instance of the <see cref="AssemblyTransaction" /> class.</summary>
    /// <param name="assemblyFactory"></param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="autoExecute">
    ///   Indicates whether the assembly code is executed once the object is
    ///   disposed.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    public AssemblyTransaction(IAssemblyFactory assemblyFactory,
                               IntPtr           address,
                               bool             autoExecute,
                               IRemoteThread    executingThread)
    {
      _assemblyFactory = assemblyFactory;
      _executingThread = executingThread;
      IsAutoExecuted   = autoExecute;
      Address          = address;
      // Initialize the string builder
      _mnemonics = new StringBuilder();
    }

    /// <summary>Initializes a new instance of the <see cref="AssemblyTransaction" /> class.</summary>
    /// <param name="assemblyFactory"></param>
    /// <param name="autoExecute">
    ///   Indicates whether the assembly code is executed once the object is
    ///   disposed.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    public AssemblyTransaction(IAssemblyFactory assemblyFactory,
                               bool             autoExecute,
                               IRemoteThread    executingThread)
      : this(assemblyFactory,
             IntPtr.Zero,
             autoExecute,
             executingThread) { }

    /// <summary>Releases all resources used by the <see cref="AssemblyTransaction" /> object.</summary>
    public virtual void Dispose()
    {
      // If a pointer was specified
      if (Address != IntPtr.Zero)
        // If the assembly code must be executed
        if (IsAutoExecuted)
          _exitCode = _assemblyFactory.InjectAndExecute<IntPtr>(_mnemonics.ToString(),
                                                               Address,
                                                               _executingThread);
        // Else the assembly code is just injected
        else
          _assemblyFactory.Inject(_mnemonics.ToString(),
                                  Address,
                                  _executingThread);

      // If no pointer was specified and the code assembly code must be executed
      if (Address == IntPtr.Zero && IsAutoExecuted)
        _exitCode = _assemblyFactory.InjectAndExecute<IntPtr>(_mnemonics.ToString(),
                                                             _executingThread);
    }

    #endregion




    #region Properties Impl - Public

    /// <summary>The address where to assembly code is assembled.</summary>
    public IntPtr Address { get; }

    /// <summary>
    ///   Gets the value indicating whether the assembly code is executed once the object is
    ///   disposed.
    /// </summary>
    public bool IsAutoExecuted { get; set; }

    #endregion




    #region Methods Impl

    /// <summary>Adds a mnemonic to the transaction.</summary>
    /// <param name="asm">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void AddLine(string          asm,
                        params object[] args)
    {
      _mnemonics.AppendLine(string.Format(asm,
                                         args));
    }

    /// <summary>Assemble the assembly code of this transaction.</summary>
    /// <returns>An array of bytes containing the assembly code.</returns>
    public byte[] Assemble()
    {
      return _assemblyFactory.Assembler.Assemble(_mnemonics.ToString());
    }

    /// <summary>Removes all mnemonics from the transaction.</summary>
    public void Clear()
    {
      _mnemonics.Clear();
    }

    /// <summary>Gets the termination status of the thread.</summary>
    public T GetExitCode<T>()
    {
      return MarshalType<T>.PtrToObject(_assemblyFactory.Process,
                                        _exitCode);
    }

    /// <summary>Inserts a mnemonic to the transaction at a given index.</summary>
    /// <param name="index">The position in the transaction where insertion begins.</param>
    /// <param name="asm">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void InsertLine(int             index,
                           string          asm,
                           params object[] args)
    {
      _mnemonics.Insert(index,
                       string.Format(asm,
                                     args));
    }

    #endregion
  }
}
