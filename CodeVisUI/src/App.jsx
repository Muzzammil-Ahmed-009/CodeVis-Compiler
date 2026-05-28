import { useState } from 'react';
import axios from 'axios';
import { Play, AlertCircle, FileCode2, CheckCircle2 } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import Tree from 'react-d3-tree';
import { Panel, Group, Separator } from 'react-resizable-panels';

function App() {
  const [code, setCode] = useState('int x = 10;\nint y = x + 5;\nprintf(y);');
  const [activeTab, setActiveTab] = useState('tokens');
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const templates = [
    { name: 'Simple Arithmetic', code: 'int x = 10;\nint y = x + 5;\nprintf(y);' },
    { name: 'Type Mismatch Error', code: 'int x = "hello";\nint y = x + 5;\nprintf(y);' },
    { name: 'Scope/Declaration Error', code: 'int x = 10;\ny = x + 5;\nprintf(y);' },
  ];

  const handleCompile = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await axios.post('https://codevis-compiler.onrender.com/api/compiler', { code }, {
        // Adjust port if necessary
        headers: { 'Content-Type': 'application/json' }
      });
      setResult(response.data);
      if (response.data.errors && response.data.errors.length > 0) {
        setActiveTab('errors');
      }
    } catch (err) {
      setError(err.message || 'Failed to connect to backend.');
    } finally {
      setLoading(false);
    }
  };

  const renderTabContent = () => {
    if (!result && !error) return <div className="text-gray-500 text-center mt-20">Click compile to see results.</div>;

    if (error) {
      return (
        <div className="p-4 bg-red-900/30 border border-red-500/50 rounded-lg flex items-start space-x-3">
          <AlertCircle className="text-red-500 mt-1" />
          <div className="text-red-200">{error}</div>
        </div>
      );
    }

    switch (activeTab) {
      case 'tokens': {
        const getTokenDetails = (token) => {
          switch (token.type) {
            case 0: return { label: 'Keyword', detail: 'Reserved word in the language' };
            case 1: return { label: 'Identifier', detail: 'Variable name' };
            case 2: 
              if (token.value === '=') return { label: 'Assignment Operator', detail: 'Assigns value to a variable' };
              return { label: 'Arithmetic Operator', detail: 'Mathematical operation' };
            case 3: 
              if (token.value.startsWith('"')) return { label: 'Literal (String)', detail: 'Text value' };
              if (token.value.includes('.')) return { label: 'Literal (Float)', detail: 'Decimal numeric value' };
              return { label: 'Literal (Integer)', detail: 'Fixed numeric value' };
            case 4: 
              if (token.value === ';') return { label: 'Punctuation / Delimiter', detail: 'Statement terminator' };
              return { label: 'Punctuation', detail: 'Syntax delimiter' };
            case 5: return { label: 'EOF', detail: 'End of File marker' };
            default: return { label: 'Unknown', detail: '' };
          }
        };

        // Group tokens by line (excluding EOF unless it's the only thing)
        const groupedTokens = result.tokens?.reduce((acc, token) => {
          if (token.type === 5 && result.tokens.length > 1) return acc; // Skip EOF for UI clarity if there are other tokens
          if (!acc[token.line]) acc[token.line] = [];
          acc[token.line].push(token);
          return acc;
        }, {});

        return (
          <div className="space-y-8 pb-10">
            {Object.entries(groupedTokens || {}).map(([lineStr, lineTokens]) => {
              // Reconstruct the line string for the header
              const lineContent = lineTokens.map(t => t.value).join(' ');
              return (
                <div key={lineStr} className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-200">
                    Line {lineStr}: <span className="px-2 py-1 bg-gray-800 rounded font-mono text-sm text-gray-300">{lineContent}</span>
                  </h3>
                  <div className="overflow-x-auto rounded-lg border border-gray-800">
                    <table className="w-full text-left border-collapse">
                      <thead>
                        <tr className="bg-[#1e293b] text-gray-400 border-b border-gray-800">
                          <th className="py-3 px-4 font-medium">Token</th>
                          <th className="py-3 px-4 font-medium">Type</th>
                          <th className="py-3 px-4 font-medium">Detail</th>
                          <th className="py-3 px-4 font-medium">Line:Col</th>
                        </tr>
                      </thead>
                      <tbody>
                        {lineTokens.map((token, i) => {
                          const info = getTokenDetails(token);
                          return (
                            <tr key={i} className="border-b border-gray-800/50 hover:bg-gray-800/30 transition-colors">
                              <td className="py-3 px-4">
                                <span className="font-mono bg-gray-800 text-gray-200 px-2 py-1 rounded text-sm">
                                  {token.value}
                                </span>
                              </td>
                              <td className="py-3 px-4 font-semibold text-primary">{info.label}</td>
                              <td className="py-3 px-4 text-gray-400 text-sm">{info.detail}</td>
                              <td className="py-3 px-4 text-gray-500 text-sm">{token.line}:{token.column}</td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                </div>
              );
            })}
          </div>
        );
      }
      case 'symbol':
        return (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-gray-700 text-gray-400">
                  <th className="py-2 px-4">Name</th>
                  <th className="py-2 px-4">Type</th>
                </tr>
              </thead>
              <tbody>
                {result.symbolTable?.map((sym, i) => (
                  <tr key={i} className="border-b border-gray-800 hover:bg-gray-800/50 transition-colors">
                    <td className="py-2 px-4 font-mono text-accent">{sym.name}</td>
                    <td className="py-2 px-4 text-primary">{sym.dataType}</td>
                  </tr>
                ))}
                {(!result.symbolTable || result.symbolTable.length === 0) && (
                  <tr><td colSpan="2" className="py-4 text-center text-gray-500">No symbols found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        );
      case 'ast': {
        if (!result.ast) return <div className="text-gray-500 text-center mt-10">AST not generated due to errors or empty code.</div>;
        
        const getNodeDetails = (node) => {
          if (node.name === 'Assignment') return { label: '=', type: 'operator' };
          if (node.name === 'VarDeclaration') return { label: node.attributes?.type || 'int', type: 'keyword' };
          if (node.name === 'PrintStatement') return { label: 'printf', type: 'keyword' };
          if (node.name === 'Program') return { label: 'Root', type: 'root' };
          if (node.name === 'BinaryOp') return { label: node.attributes?.value || '?', type: 'operator' };
          if (node.name === 'Literal') return { label: node.attributes?.value || '', type: 'literal' };
          if (node.name === 'Identifier') return { label: node.attributes?.value || '', type: 'identifier' };
          return { label: node.name, type: 'default' };
        };

        const getColors = (type) => {
           switch(type) {
             case 'root': return { bg: '#6d28d9', text: '#ffffff', stroke: '#8b5cf6' }; // Purple
             case 'operator': return { bg: '#2563eb', text: '#ffffff', stroke: '#60a5fa' }; // Blue
             case 'literal': return { bg: '#059669', text: '#ffffff', stroke: '#34d399' }; // Emerald
             case 'identifier': return { bg: '#d97706', text: '#ffffff', stroke: '#fbbf24' }; // Amber
             case 'keyword': return { bg: '#db2777', text: '#ffffff', stroke: '#f472b6' }; // Pink
             default: return { bg: '#1e293b', text: '#e2e8f0', stroke: '#3b82f6' };
           }
        };

        return (
          <div className="h-full w-full min-h-[500px] bg-[#0b1121] rounded-lg overflow-hidden border border-gray-800 shadow-inner" id="treeWrapper">
             <Tree 
                data={result.ast} 
                orientation="vertical"
                pathFunc="straight"
                translate={{ x: 350, y: 50 }}
                nodeSize={{ x: 120, y: 120 }}
                renderCustomNodeElement={(rd3tProps) => {
                  const { nodeDatum, toggleNode } = rd3tProps;
                  const details = getNodeDetails(nodeDatum);
                  const colors = getColors(details.type);
                  
                  return (
                    <g className="cursor-pointer" onClick={toggleNode}>
                      <circle 
                        r="28" 
                        fill={colors.bg} 
                        stroke={colors.stroke} 
                        strokeWidth="3" 
                        className="transition-all duration-300 hover:opacity-80"
                      />
                      <text 
                        fill={colors.text} 
                        style={{ fill: colors.text, stroke: 'none', pointerEvents: 'none' }}
                        x="0" 
                        y="0" 
                        textAnchor="middle" 
                        alignmentBaseline="central" 
                        dominantBaseline="central"
                        className="font-mono font-bold select-none"
                        fontSize={details.type === 'operator' ? '28px' : '16px'}
                      >
                        {details.label}
                      </text>
                    </g>
                  );
                }}
             />
          </div>
        );
      }
      case 'tac':
        return (
          <div className="space-y-2 font-mono">
            {result.tac?.map((instruction, i) => (
              <div key={i} className="p-2 bg-gray-800 rounded text-gray-300 border border-gray-700">
                <span className="text-gray-500 mr-4">{(i + 1).toString().padStart(2, '0')}</span> {instruction}
              </div>
            ))}
            {(!result.tac || result.tac.length === 0) && (
              <div className="text-center text-gray-500 py-4">No TAC generated.</div>
            )}
          </div>
        );
      case 'optimized':
        return (
          <div className="space-y-2 font-mono">
            {result.optimizedTac?.map((instruction, i) => (
              <div key={i} className="p-2 bg-[#1e293b] rounded text-[#34d399] border border-gray-700 font-bold">
                <span className="text-gray-500 mr-4 font-normal">{(i + 1).toString().padStart(2, '0')}</span> {instruction}
              </div>
            ))}
            {(!result.optimizedTac || result.optimizedTac.length === 0) && (
              <div className="text-center text-gray-500 py-4">No optimized code generated.</div>
            )}
          </div>
        );
      case 'assembly':
        return (
          <div className="space-y-2 font-mono">
            {result.assemblyCode?.map((instruction, i) => (
              instruction.trim() === "" ? <div key={i} className="h-2"></div> :
              <div key={i} className="p-2 bg-gray-900 rounded text-gray-200 border border-gray-800">
                <span className="text-gray-500 mr-4 font-normal">{(i + 1).toString().padStart(2, '0')}</span> {instruction}
              </div>
            ))}
            {(!result.assemblyCode || result.assemblyCode.length === 0) && (
              <div className="text-center text-gray-500 py-4">No assembly code generated.</div>
            )}
          </div>
        );
      case 'errors':
        return (
          <div className="space-y-3">
            {result.errors?.map((err, i) => (
              <div key={i} className="p-4 bg-red-900/30 border border-red-500/50 rounded-lg flex items-start space-x-3">
                <AlertCircle className="text-red-500 mt-1 shrink-0" />
                <div className="text-red-200 font-mono">{err}</div>
              </div>
            ))}
            {(!result.errors || result.errors.length === 0) && (
              <div className="flex flex-col items-center justify-center py-10 space-y-4">
                <CheckCircle2 className="w-16 h-16 text-green-500" />
                <span className="text-green-400 text-lg">Compilation successful. No errors found.</span>
              </div>
            )}
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="h-screen bg-background text-gray-100 flex flex-col font-sans overflow-hidden">
      <header className="bg-surface border-b border-gray-800 p-4 flex justify-between items-center z-10 shadow-lg">
        <div className="flex items-center space-x-3">
          <h1 className="text-xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
            CodeVis Compiler
          </h1>
        </div>
        <div className="flex items-center space-x-4">
           <select 
              className="bg-gray-800 border border-gray-700 text-sm rounded-md px-3 py-2 outline-none focus:border-primary"
              onChange={(e) => setCode(templates[parseInt(e.target.value)].code)}
              defaultValue=""
            >
              <option value="" disabled>Load Template...</option>
              {templates.map((t, i) => <option key={i} value={i}>{t.name}</option>)}
           </select>
          <button
            onClick={handleCompile}
            disabled={loading}
            className="bg-primary hover:bg-blue-600 text-white px-6 py-2 rounded-md font-medium transition-colors flex items-center space-x-2 disabled:opacity-50"
          >
            {loading ? <span className="animate-spin mr-2">⟳</span> : <Play className="w-4 h-4" />}
            <span>Compile</span>
          </button>
        </div>
      </header>

      <main className="flex-1 flex overflow-hidden">
        <Group direction="horizontal">
          <Panel defaultSize={50} minSize={30} className="flex flex-col bg-surface">
            <div className="p-3 border-b border-gray-800 flex items-center text-gray-400 text-sm font-medium">
              <FileCode2 className="w-4 h-4 mr-2" />
              <span>Source Code (subset of C)</span>
            </div>
            <textarea
              value={code}
              onChange={(e) => setCode(e.target.value)}
              className="flex-1 bg-transparent p-6 font-mono text-base resize-none outline-none text-gray-100 leading-relaxed"
              spellCheck="false"
              placeholder="// Write your code here..."
            />
          </Panel>

          <Separator className="w-1 bg-gray-800 hover:bg-primary/50 cursor-col-resize transition-colors duration-200" />

          <Panel defaultSize={50} minSize={30} className="flex flex-col bg-[#0b1121]">
            <div className="flex border-b border-gray-800 bg-surface overflow-x-auto">
              {[
                { id: 'tokens', label: 'Lexical Analysis' },
                { id: 'ast', label: 'Syntax Analyzer' },
                { id: 'symbol', label: 'Semantic Analyzer' },
                { id: 'tac', label: 'Intermediate Code' },
                { id: 'optimized', label: 'Optimized Code' },
                { id: 'assembly', label: 'Code Generation' },
                { id: 'errors', label: 'Errors' }
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`px-6 py-3 text-sm font-medium transition-all border-b-2 whitespace-nowrap ${
                    activeTab === tab.id 
                      ? 'border-primary text-primary bg-primary/5' 
                      : 'border-transparent text-gray-500 hover:text-gray-300 hover:bg-gray-800/50'
                  }`}
                >
                  {tab.label}
                  {tab.id === 'errors' && result?.errors?.length > 0 && (
                    <span className="ml-2 bg-red-500 text-white text-[10px] px-1.5 py-0.5 rounded-full">
                      {result.errors.length}
                    </span>
                  )}
                </button>
              ))}
            </div>
            <div className="flex-1 p-6 overflow-y-auto relative">
              <AnimatePresence mode="wait">
                <motion.div
                  key={activeTab}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -10 }}
                  transition={{ duration: 0.2 }}
                  className="h-full"
                >
                  {renderTabContent()}
                </motion.div>
              </AnimatePresence>
            </div>
          </Panel>
        </Group>
      </main>
    </div>
  );
}

export default App;
