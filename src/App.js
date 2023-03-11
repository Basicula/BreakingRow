import { useState } from "react";

import GameField from "./GameField.js";
import Statistics from "./Statistics.js";

import './style/App.css';

function App() {
  const [score, set_score] = useState(0);
  const [elements_count, set_elements_count] = useState({});

  const update_statistics = (value, count) => {
    set_score(score + value * count);
    var new_elements_count = elements_count;
    if (value in elements_count)
      new_elements_count[value] += count;
    else
      new_elements_count[value] = count;
    set_elements_count(new_elements_count);
  }

  return (
    <div className="app-container">
      <Statistics score={score} elements_count={elements_count} />
      <GameField width={7} height={7} onStrike={update_statistics} />
    </div>
  );
}

export default App;
