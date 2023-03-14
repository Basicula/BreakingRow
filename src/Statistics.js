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
          Object.keys(statistics[current_tab]).map((info_name, i) => {
            return (
              <InfoContainer key={info_name}>
                <InfoNameContainer>{info_name}</InfoNameContainer>
                :
                <InfoDataContainer>{statistics[current_tab][info_name]}</InfoDataContainer>
              </InfoContainer>
            );
          })}
        {Object.keys(statistics[current_tab]).length === 0 &&
          <NoDataContainer>No data</NoDataContainer>}
      </TabContentContainer>
    </StatisticsContainer>
  );
}

const StatisticsContainer = styled.div`
  min-width: 100px;

  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  border: 1px solid black;
  border-radius: 5px;
`;

const TabsContainer = styled.div`
  display: flex;
  flex-direction: row;
`;

const TabContainer = styled.div`
  padding: 5px;
  box-sizing: border-box;
  
  border-radius: 5px;
  color: white;
  background: black;

  cursor: pointer;
`;

const SelectedTabContainer = styled.div`
  padding: 5px;
  box-sizing: border-box;

  border-radius: 5px;
  color: white;
  background: grey;

  cursor: default;
`;

const TabContentContainer = styled.div`
  width: 100%;
  display: flex;
  flex-direction: column;
  padding: 5px;
  box-sizing: border-box;

  border: 2px solid black;
  border-radius: 5px;
`;

const InfoContainer = styled.div`
  display: flex;
  flex-direction: row;

  justify-content: center;
`;

const InfoNameContainer = styled.div`
  margin-right: 5px;
`;
const InfoDataContainer = styled.div`
  width: 100%;
  margin-left: 5px;
`;

const NoDataContainer = styled.div`
  width: 100%;
  text-align: center;
`;