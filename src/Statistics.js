import { useState } from 'react';
import styled from 'styled-components'

export default function Statistics({ elements_count, strikes_statistics }) {
  const [current_tab, set_current_tab] = useState(0);

  const statistics = [elements_count, strikes_statistics]
  const tab_names = ["Elements count", "Strikes statistics"];

  return (
    <StatisticsContainer>
      <TabsContainer>
        {tab_names.map((tab_name, i) => {
          if (i === current_tab)
            return (
              <SelectedTabContainer key={i} onClick={() => set_current_tab(i)}>
                {tab_name}
              </SelectedTabContainer>
            );
          return (<TabContainer key={i} onClick={() => set_current_tab(i)}>{tab_name}</TabContainer>);
        })}
      </TabsContainer>
      <TabContentContainer>
        {Object.keys(statistics[current_tab]).length > 0 &&
          Object.keys(statistics[current_tab]).map((element, i) => {
            return (
              <ElementCountInfo key={element}>
                <ElementContainer>{element}</ElementContainer>
                :
                <ElementCountContainer>{statistics[current_tab][element]}</ElementCountContainer>
              </ElementCountInfo>
            );
          })}
        {Object.keys(statistics[current_tab]).length === 0 && (<div>No data</div>)}
      </TabContentContainer>
    </StatisticsContainer>
  );
}

const StatisticsContainer = styled.div`
  height: 100%;
  min-width: 100px;

  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
`;

const TabsContainer = styled.div`
  display: flex;
  flex-direction: row;
`;

const TabContainer = styled.div`
  padding: 5px;
  box-sizing: border-box;
  
  border: 1px solid white;
  color: white;
  background: black;

  cursor: pointer;
`;

const SelectedTabContainer = styled.div`
  padding: 5px;
  box-sizing: border-box;

  border: 1px solid white;
  color: white;
  background: grey;

  cursor: default;
`;

const TabContentContainer = styled.div`
`;

const ElementCountInfo = styled.div`
  display: flex;
  flex-direction: row;

  text-align: center;
`;

const ElementContainer = styled.div``;
const ElementCountContainer = styled.div``;
