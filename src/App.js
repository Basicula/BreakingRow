import { useRef, useEffect } from "react";

import GameField from "./GameField.js";

import './style/App.css';

function App() {
  const score_ref = useRef(null);
  const elements_count_ref = useRef(null);

  var elements_count = {};

  const update_elements_count = (value, count) => {
    if (value in elements_count)
      elements_count[value] += count;
    else
      elements_count[value] = count;
    const elements_counts_container = elements_count_ref.current;

  }

  return (
    <div className="app-container">
      <div className="stats-container">
        <div className="score-container">
          Score
          <div
            className="score-wrapper"
            ref={score_ref}>
            0
          </div>
        </div>
        <div
          className="elements-count-container"
          ref={elements_count_ref}>
        </div>
      </div>
      <GameField width={7} height={7}/>
    </div>
  );
}

export default App;
