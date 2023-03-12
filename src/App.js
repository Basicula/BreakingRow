import { useState, useEffect } from "react";
import styled from 'styled-components'

import GameField from "./GameField.js";
import Statistics from "./Statistics.js";

export default function App() {
  const [window_size, set_window_size] = useState([]);
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

  useEffect(() => {
    function updateSize() {
      set_window_size([window.innerWidth, window.innerHeight]);
    }
    window.addEventListener('resize', updateSize);
    updateSize();
    return () => window.removeEventListener('resize', updateSize);
  }, []);

  return (
    <AppContainer>
      <Statistics elements_count={elements_count} />
      <GameFieldContainer>
        <ScoreContainer>
          <ScoreTitleContainer>Score</ScoreTitleContainer>
          <ScoreValueContainer>{score}</ScoreValueContainer>
        </ScoreContainer>
        <GameField width={7} height={7} onStrike={update_statistics} />
      </GameFieldContainer>
    </AppContainer>
  );
}

const AppContainer = styled.div`
  width: 100%;
  height: 100%;

  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
`;

const ScoreContainer = styled.div`
  width: 100%;

  display: flex;
  flex-direction: row;
  justify-content: space-between;
  padding: 5px;
  margin-bottom: 2px;
  box-sizing: border-box;

  background: #aaa;

  font-size: 48px;

  border-radius: 10px;
`;

const ScoreTitleContainer = styled.div`
  font-weight: bold;
  text-shadow: 2px 2px 5px white;
`;
const ScoreValueContainer = styled.div`
  font-weight: bold;
  text-shadow: 2px 2px 5px white;
`;

const GameFieldContainer = styled.div`
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 5px; 
`;
